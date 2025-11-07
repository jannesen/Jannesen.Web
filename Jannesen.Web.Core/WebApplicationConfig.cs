using System;
using System.Collections.Generic;
using System.IO;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core
{
    public sealed class WebApplicationConfig: IDisposable
    {
#if DEBUG
        private     const               int                                                         DependanceCheckTime     = 1;
#else
        private     const               int                                                         DependanceCheckTime     = 5;
#endif
        private sealed class DependanceFile
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

        private     readonly            WebApplication                      _application;
        private     readonly            List<DependanceFile>                _dependanceFiles;
        private     readonly            CoreHttpHandlerDictionary           _httpHandlers;
        private     readonly            CoreResourceDictionary              _resources;
        private                         DateTime                            _nextDependanceCheck;

        public                          WebApplication                      Application     => _application;

        public                                                              WebApplicationConfig(WebApplication application)
        {
            _application         = application;
            _dependanceFiles     = new List<DependanceFile>(32);
            _httpHandlers        = new CoreHttpHandlerDictionary();
            _resources           = new CoreResourceDictionary();
            _nextDependanceCheck = DateTime.UtcNow.AddTicks(TimeSpan.TicksPerSecond * DependanceCheckTime);
        }
        public                          void                                Dispose()
        {
            _resources.Dispose();
        }

        public                          void                                Load(IServiceProvider serviceProvider, string directory)
        {
            try {
                var rtn = 0;

                if (_loadConfig(serviceProvider, "/", directory + "\\jannesen.web.config") < 0) {
                    rtn = -1;
                }

                if (rtn != 0) {
                    _application.LogError("Initialized with errors");
                }
                else {
                    _application.LogInfo("Initialized");
                }
            }
            catch(Exception err) {
                _application.LogError("Initialization failed." , err);
            }
        }
        public                          bool                                NeedsReInitialize()
        {
            if (_nextDependanceCheck < DateTime.UtcNow) {
                for(var i = 0 ; i < _dependanceFiles.Count ; ++i) {
                    if (_dependanceFiles[i].IsChanged)
                        return true;
                }
            }

            _nextDependanceCheck = DateTime.UtcNow.AddTicks(TimeSpan.TicksPerSecond * DependanceCheckTime);

            return false;
        }

        public                          WebCoreHttpHandler?                 GetHttpHandler(string path, string verb)
        {
            return _httpHandlers.GetHandler(path, verb);
        }

        public                          T                                   GetResource<T>(string name) where T: WebCoreResource
        {
            return (T)_resources.GetWebResource(typeof(T), name);
        }

        private                         int                                 _loadConfig(IServiceProvider serviceProvider, string? path, string filename)
        {
            var rtn = 0;

            System.Diagnostics.Debug.WriteLine("LoadConfiguration: " + filename);

            try {
                using (var configReader = new WebCoreConfigReader(serviceProvider, this, path, filename)) {
                    _dependanceFiles.Add(new DependanceFile(configReader.Filename));

                    configReader.ReadRootNode("configuration");

                    while (configReader.ReadNextElement()) {
                        try {
                            switch(configReader.ElementName) {
                            case "name": {
                                    configReader.NoChildElements();
                                }
                                break;

                            case "load": {
                                    var name = configReader.GetValueString("name");
                                    configReader.NoChildElements();
                                    WebLoader.Instance.LoadModule(name);
                                }
                                break;

                            case "http-handler":
                                _addHttpHandler((WebCoreHttpHandler)WebLoader.Instance.ConstructDynamicClass(new WebCoreHttpHandlerAttribute(configReader.GetValueString("type")), configReader));
                                break;

                            case "resource":
                                _addResource((WebCoreResource)WebLoader.Instance.ConstructDynamicClass(new WebCoreResourceAttribute(configReader.GetValueString("type")), configReader));
                                break;

                            case "include": {
                                    var file    = configReader.GetValueString("file").Replace("\\", "/", StringComparison.Ordinal);
                                    var incpath = (string?)null;

                                    configReader.NoChildElements();

                                    if (file.IndexOf("./", StringComparison.Ordinal) < 0 && path != null) {
                                        var i = file.LastIndexOf('/');
                                        incpath = (i >= 0) ? string.Concat(path, file.AsSpan(0, i + 1)) : path;
                                    }

                                    if (_loadConfig(serviceProvider, incpath, configReader.CombinePhysicalPath(file)) < 0) {
                                        rtn = -1;
                                    }
                                }
                                break;

                            default:
                                throw new WebConfigException("Unknown configuration '" + configReader.ElementName + "' entry.", configReader);
                            }
                        }
                        catch(Exception err) {
                            while (err is System.Reflection.TargetInvocationException && err.InnerException != null) {
                                err = err.InnerException;
                            }

                            if (err is not WebConfigException)
                                err = new WebConfigException("Parsing configuration entry failed.", err, configReader);

                            _application.LogError(null, err);

                            configReader.Reset();
                        }
                    }

                    configReader.ReadEOF();
                }

                return rtn;
            }
            catch(Exception err) {
                _application.LogError("Load configuration file '" + filename + "' failed.", err);
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
    }
}
