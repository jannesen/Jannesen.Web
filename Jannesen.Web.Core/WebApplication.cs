using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core
{
    public class WebApplication
    {
        public  enum    EventID
        {
            Error               = 1,
            ReInitialize        = 1000,
            Startup             = 1001,
            StartupWithError    = 1002,
            NewLogfile          = 1010,
            Info                = 1100,
            Warning             = 1200,
            DeadLockWarning     = 1201
        }
        public enum     AppStatus
        {
            None        = 0,
            Initializing,
            Initialized,
            InitFailed
        }

#if DEBUG
        private     const               int                                                         DependanceCheckTime     = 1;
#else
        private     const               int                                                         DependanceCheckTime     = 5;
#endif
        private class DependanceFile
        {
            public      string      Filename;
            public      DateTime    LastModified;

            public      bool        IsChanged
            {
                get {
                    return LastModified != GetLastModified();
                }
            }
            public                  DependanceFile(string fileName)
            {
                this.Filename     = fileName;
                this.LastModified = GetLastModified();
            }

            private     DateTime    GetLastModified()
            {
                return new FileInfo(this.Filename).LastWriteTimeUtc;
            }
        }

        private     static              object                                                      _appLock      = new object();
        private     static              WebApplication                                              _application  = null;
        private     static              string                                                      _name         = "Jannesen.Web";

        private                         AppStatus                                                   _status;
        private                         string                                                      _basePath;
        private                         string                                                      _appPath;
        private                         List<DependanceFile>                                        _dependanceFiles;
        private                         List<Assembly>                                              _loadedModules;
        private                         Dictionary<WebCoreAttribureDynamicClass, ConstructorInfo>   _dynamicClasses;
        private                         CoreHttpHandlerDictionary                                   _httpHandlers;
        private                         CoreResourceDictionary                                      _resources;
        private                         DateTime                                                    _nextDependanceCheck;

        public      static              string                              Name
        {
            get {
                return _name;
            }
        }
        public                          AppStatus                           Status
        {
            get {
                return _status;
            }
        }

        public      static              WebApplication                      GetApplication()
        {
            lock(_appLock)
            {
                if (_application == null || _application.Status != AppStatus.Initialized)
                    throw new WebAppNotInitialized();

                if (_application.NeedsReInitialize()) {
                    InitializeAppl();

                    if (_application == null || _application.Status != AppStatus.Initialized)
                        throw new WebAppNotInitialized();
                }

                return _application;
            }
        }

        public      static              void                                LogError(WebCoreHttpHandler handler, WebCoreCall httpCall, Exception exception)
        {
            string message = "Error in handler:\r\n" +
                         "  " + handler.Path + "\r\n\r\n" +
                         "Url:\r\n" +
                         "  " + httpCall.Request.Url.ToString();

            if (exception is System.Data.SqlClient.SqlException)
                LogSqlException(message, (System.Data.SqlClient.SqlException)exception);
            else
                LogError(message, exception);
        }
        public      static              void                                LogSqlException(string message, System.Data.SqlClient.SqlException exception)
        {
            if (exception.Errors.Count > 0) {
                EventID id = EventID.Warning;

                if (!message.EndsWith("\n"))
                    message += "\r\n";

                message += "\r\nSqlError(s):\r\n" ;

                foreach(System.Data.SqlClient.SqlError err in exception.Errors) {
                    if (err.Class > 12)
                        id = EventID.Error;

                    message += "  ";

                    if (!string.IsNullOrEmpty(err.Procedure))
                        message += err.Procedure + "(" + err.LineNumber + "): ";

                    message += "#" + err.Number.ToString() + " " + err.Message + "\r\n";
                }

                LogEvent(id, message);
            }
            else
                LogError(message, exception);
        }
        public      static              void                                LogError(string message, Exception exception)
        {
            LogEvent(EventID.Error, message, exception);
        }
        public      static              void                                LogEvent(EventID id, string message, Exception exception)
        {
            Exception ex;

            if (exception != null) {
                if (!message.EndsWith("\n"))
                    message += "\r\n";

                message += "\r\nException(s):\r\n" ;

                for (ex = exception ; ex != null ; ex = ex.InnerException) {
                    if (ex is System.Reflection.TargetInvocationException)
                        continue;

                    string          m            = ex.Message;

                    if (ex is IFileLocation fileLocation) {
                        if (fileLocation !=null && fileLocation.Filename != null)
                            m = fileLocation.Filename + "(" + fileLocation.LineNumber.ToString() + "): " + m;
                    }

                    message += "  " + m + "\r\n";
                }

                ex = exception;

                while (ex.InnerException != null)
                    ex = ex.InnerException;

                if (ex is System.AccessViolationException               ||
                    ex is System.AppDomainUnloadedException             ||
                    ex is System.ArgumentException                      ||
                    ex is System.ArithmeticException                    ||
                    ex is System.ArrayTypeMismatchException             ||
                    ex is System.BadImageFormatException                ||
                    ex is System.CannotUnloadAppDomainException         ||
                    ex is System.IndexOutOfRangeException               ||
                    ex is System.InsufficientExecutionStackException    ||
                    ex is System.InvalidCastException                   ||
                    ex is System.InvalidOperationException              ||
                    ex is System.InvalidProgramException                ||
                    ex is System.NotImplementedException                ||
                    ex is System.NotSupportedException                  ||
                    ex is System.NullReferenceException                 ||
                    ex is System.OperationCanceledException             ||
                    ex is System.OutOfMemoryException                   ||
                    ex is System.UnauthorizedAccessException)
                    message += "\r\nStacktrace:\r\n" + ex.StackTrace;
            }

            LogEvent(id, message);
        }
        public      static              void                                LogEvent(EventID id, string message)
        {
            System.Diagnostics.Debug.WriteLine("LOG: "+message);

            using (SystemSection systemSection = new SystemSection())
            {
                systemSection.ToSystem();

                try {
                    using (System.Diagnostics.EventLog  eventLog = new System.Diagnostics.EventLog())
                    {
                        eventLog.Source = "Jannesen.Web";
                        eventLog.Log    = "Application";

                        System.Diagnostics.EventLogEntryType    type  = System.Diagnostics.EventLogEntryType.Error;

                        switch(id)
                        {
                        case EventID.Startup:           type = System.Diagnostics.EventLogEntryType.Information;    break;
                        case EventID.StartupWithError:  type = System.Diagnostics.EventLogEntryType.Warning;        break;
                        case EventID.NewLogfile:        type = System.Diagnostics.EventLogEntryType.Information;    break;
                        case EventID.ReInitialize:      type = System.Diagnostics.EventLogEntryType.Information;    break;
                        case EventID.Error:             type = System.Diagnostics.EventLogEntryType.Error;          break;
                        case EventID.Warning:           type = System.Diagnostics.EventLogEntryType.Warning;        break;
                        case EventID.DeadLockWarning:   type = System.Diagnostics.EventLogEntryType.Warning;        break;
                        case EventID.Info:              type = System.Diagnostics.EventLogEntryType.Information;    break;
                        default:                        type = System.Diagnostics.EventLogEntryType.Error;          break;
                        }

                        eventLog.WriteEntry(_name + ": " + message, type, (int)id);
                    }
                }
                catch(Exception) {
                }
            }
        }

        public      static              WebCoreHttpHandler                  GetHttpHandler(string path, string verb)
        {
            return GetApplication().waGetHttpHandler(path, verb);
        }
        public      static              T                                   GetResource<T>(string name) where T: WebCoreResource
        {
            T   rtn = (T)GetApplication().waGetResource<T>(name);

            if (rtn.Down)
                throw new WebResourceDownException("Resource '" + rtn.Name + "' down.");

            return rtn;
        }
        public      static              string                              GetRelPath(string path)
        {
            lock(_appLock)
            {
                if (_application == null)
                    throw new WebAppNotInitialized();

                return _application.waGetRelPath(path);
            }
        }
        public      static              WebCoreDataSource                   GetDataSource(string source, string name)
        {
            lock(_appLock)
            {
                if (_application == null)
                    throw new WebAppNotInitialized();

                return _application.waGetDataSource(source, name);
            }
        }
        public      static              object                              ConstructDynamicClass(WebCoreAttribureDynamicClass className, params object[] args)
        {
            lock(_appLock)
            {
                if (_application == null)
                    throw new WebAppNotInitialized();

                return _application.waConstructDynamicClassArgs(className, args);
            }
        }

        public      static              void                                AppDomainInitialize()
        {
            lock(_appLock)
            {
                if (_application == null)
                    InitializeAppl();
            }
        }
        public      static              void                                AppDomainUnload()
        {
            lock(_appLock)
            {
                if (_application != null) {
                    _application.Unload();
                    _application = null;
                }
            }
        }

        public                                                              WebApplication(string basePath, string appPath)
        {
            _status              = AppStatus.None;
            _basePath            = basePath;
            _appPath             = appPath;
            _dependanceFiles     = new List<DependanceFile>(32);
            _loadedModules       = new List<Assembly>();
            _dynamicClasses      = new Dictionary<WebCoreAttribureDynamicClass, ConstructorInfo>(256);
            _httpHandlers        = new CoreHttpHandlerDictionary();
            _resources           = new CoreResourceDictionary();
            _nextDependanceCheck = DateTime.UtcNow.AddTicks(TimeSpan.TicksPerSecond * DependanceCheckTime);

            if (!_basePath.EndsWith("/"))
                _basePath += "/";
        }

        public                          WebCoreHttpHandler                  waGetHttpHandler(string path, string verb)
        {
            return _httpHandlers.GetHandler(waGetRelPath(path), verb);
        }
        public                          T                                   waGetResource<T>(string name) where T: WebCoreResource
        {
            return (T)_resources.GetWebResource(typeof(T), name);
        }
        public                          string                              waGetRelPath(string path)
        {
            if (string.Compare(path, 0, _basePath, 0, _basePath.Length, true) != 0)
                throw new InternalErrorException("Requested path not in application");

            return path.Substring(_basePath.Length-1);
        }
        public                          WebCoreDataSource                   waGetDataSource(string source, string name)
        {
            if (source.IndexOf(Jannesen.Web.Core.Impl.Source.multiple.SplitChar) >= 0)
                return new Jannesen.Web.Core.Impl.Source.multiple(source, name);

            int         sep     = source.IndexOf(":");

            if (sep > 0) {
                name   = source.Substring(sep + 1);
                source = source.Substring(0, sep);
            }
            else {
                if (name == null)
                    throw new WebSourceException("Invalid source '" + source + "', name missing.");
            }

            return (WebCoreDataSource)waConstructDynamicClass(new WebCoreAttributeDataSource(source), name);
        }
        public                          object                              waConstructDynamicClass(WebCoreAttribureDynamicClass className, params object[] args)
        {
            return waConstructDynamicClassArgs(className, args);
        }
        public                          object                              waConstructDynamicClassArgs(WebCoreAttribureDynamicClass className, object[] args)
        {
            if (!_dynamicClasses.TryGetValue(className, out var constructorInfo)) {
                if (className.Name.IndexOf('.') > 0) {
                    for (int i = 0 ; i < _loadedModules.Count ; ++i) {
                        Type        classType = _loadedModules[i].GetType(className.Name);

                        if (classType != null) {
                            constructorInfo = className.GetConstructor(classType);
                            break;
                        }
                    }
                }
            }

            if (constructorInfo == null)
                throw new KeyNotFoundException("Unknown " + className.ToString() + ".");

            return constructorInfo.Invoke(args);
        }

        protected   static              void                                InitializeAppl()
        {
            if (_application != null) {
                _application.Unload();
                LogEvent(EventID.ReInitialize, "Reinitialize");
            }

            _application = new WebApplication(HttpRuntime.AppDomainAppVirtualPath, HttpRuntime.AppDomainAppPath);
            _application.LoadConfiguration();
        }
        protected                       bool                                NeedsReInitialize()
        {
            if (_nextDependanceCheck < DateTime.UtcNow) {
                using (SystemSection systemSection = new SystemSection())
                {
                    systemSection.ToSystem();

                    for(int i = 0 ; i < _dependanceFiles.Count ; ++i) {
                        if (_dependanceFiles[i].IsChanged)
                            return true;
                    }
                }
            }

            _nextDependanceCheck = DateTime.UtcNow.AddTicks(TimeSpan.TicksPerSecond * DependanceCheckTime);

            return false;
        }
        protected                       void                                LoadConfiguration()
        {
            using (SystemSection systemSection = new SystemSection())
            {
                systemSection.ToSystem();

                try {
                    int rtn = 0;

                    _status = AppStatus.Initializing;

                    _loadModuleDynamicClasses(this.GetType().Assembly);

                    if (_loadDirectoryConfig(HostingEnvironment.VirtualPathProvider.GetDirectory(_basePath)) < 0)
                        rtn = -1;

                    _status = AppStatus.Initialized;

                    if (rtn != 0)
                        LogEvent(EventID.StartupWithError, "Initialized with errors");
                    else
                        LogEvent(EventID.Startup, "Initialized");
                }
                catch(Exception err) {
                    _status = AppStatus.InitFailed;
                    LogError("Initialization failed." , err);
                }
            }
        }
        protected                       void                                Unload()
        {
            _resources.Unload();
        }

        private                         int                                 _loadDirectoryConfig(VirtualDirectory vdir)
        {
            int     rtn = 0;

            try {
                {
                    string filename = HostingEnvironment.MapPath(vdir.VirtualPath) + @"jannesen.web.config";

                    if (File.Exists(filename)) {
                        if (_loadConfig(vdir.VirtualPath, filename) < 0)
                            rtn = -1;
                    }
                }

                foreach(VirtualDirectory subvdir in vdir.Directories)
                    _loadDirectoryConfig(subvdir);
            }
            catch(Exception err) {
                LogError("Failed to load directory '" + vdir.VirtualPath + "'.", err);
                return -1;
            }

            return rtn;
        }
        private                         int                                 _loadConfig(string abspath, string filename)
        {
            int     rtn = 0;

            System.Diagnostics.Debug.WriteLine("LoadConfiguration: " + filename);

            try {
                using (WebCoreConfigReader configReader = new WebCoreConfigReader(this, (abspath != null ? waGetRelPath(abspath) : null), filename))
                {
                    _dependanceFiles.Add(new DependanceFile(configReader.Filename));

                    configReader.ReadRootNode("configuration");

                    while (configReader.ReadNextElement()) {
                        try {
                            switch(configReader.ElementName)
                            {
                            case "name":
                                {
                                    _name = configReader.GetValueString("name");
                                    configReader.NoChildElements();
                                }
                                break;

                            case "load":
                                {
                                    string name = configReader.GetValueString("name");
                                    configReader.NoChildElements();

                                    if (_loadModule(name) < 0)
                                        rtn = -1;
                                }
                                break;

                            case "http-handler":
                                _addHttpHandler((WebCoreHttpHandler)waConstructDynamicClass(new WebCoreAttribureHttpHandler(configReader.GetValueString("type")), configReader));
                                break;

                            case "resource":
                                _addResource((WebCoreResource)waConstructDynamicClass(new WebCoreAttribureResource(configReader.GetValueString("type")), configReader));
                                break;

                            case "include":
                                {
                                    string  file = configReader.GetValueString("file").Replace("\\", "/");
                                    configReader.NoChildElements();

                                    if (file.StartsWith("../") || abspath == null) {
                                        if (_loadConfig(null, configReader.CombinePhysicalPath(file)) < 0)
                                            rtn = -1;
                                    }
                                    else {
                                        int         i     = file.LastIndexOf('/');
                                        string      vdir  = abspath + file.Substring(0, i+1);
                                        string      fname = HostingEnvironment.MapPath(vdir + file.Substring(i+1));

                                        if (_loadConfig(vdir, fname) < 0)
                                            rtn = -1;
                                    }
                                }
                                break;

                            default:
                                throw new WebConfigException("Unknown configuration '" + configReader.ElementName + "' entry.", configReader);
                            }
                        }
                        catch(Exception err) {
                            while (err is System.Reflection.TargetInvocationException)
                                err = err.InnerException;

                            if (err is WebConfigException)
                                throw;

                            throw new WebConfigException("Parsing configuration entry failed.", err, configReader);
                        }
                    }

                    configReader.ReadEOF();
                }

                return rtn;
            }
            catch(Exception err) {
                LogError("Load configuration file '" + filename + "' failed.", err);
                return -1;
            }
        }
        private                         int                                 _loadModule(string name)
        {
            try {
                Assembly    assembly = Assembly.Load(name);

                if (!_loadedModules.Contains(assembly)) {
                    System.Diagnostics.Debug.WriteLine("Load assembly: " + assembly.FullName);
                    _loadedModules.Add(assembly);
                    _loadModuleDynamicClasses(assembly);
                }

                return 0;
            }
            catch(Exception err) {
                LogError("Loading assembly '" + name + "' failed.", err);
                return -1;
            }
        }
        private                         void                                _addHttpHandler(WebCoreHttpHandler coreHttpHandler)
        {
            System.Diagnostics.Debug.WriteLine("Add HttpHandler: " + coreHttpHandler.Path + " [" + coreHttpHandler.Verb + "]");
            _httpHandlers.Add(coreHttpHandler);
        }
        private                         void                                _addResource(WebCoreResource coreResource)
        {
            System.Diagnostics.Debug.WriteLine("Add Resource: " + coreResource.Type + "/" + coreResource.Name);
            _resources.Add(coreResource);
        }
        private                         void                                _loadModuleDynamicClasses(Assembly assembly)
        {
            foreach(Type type in assembly.GetTypes()) {
                try {
                    foreach(WebCoreAttribureDynamicClass attr in type.GetCustomAttributes(typeof(WebCoreAttribureDynamicClass), false))
                        _dynamicClasses.Add(attr, attr.GetConstructor(type));
                }
                catch(Exception err) {
                    throw new WebInitializationException("Failed to process class '" + type.FullName + "'.", err);
                }
            }
        }
        private     static              string[]                            _directories(string path)
        {
            List<string>        names = new List<string>();

            foreach(DirectoryInfo info in (new DirectoryInfo(path)).GetDirectories())
                names.Add(info.Name);

            names.Sort();

            return names.ToArray();
        }
    }
}
