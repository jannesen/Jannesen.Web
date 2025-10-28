using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Jannesen.Web.Core.Impl;


namespace Jannesen.Web.Core
{
    public sealed class WebLoader
    {
        public  static  readonly        WebLoader          Instance = new WebLoader();

        private     readonly            List<Assembly>                                              _loadedModules;
        private     readonly            Dictionary<WebCoreDynamicClassAttribute, ConstructorInfo>   _dynamicClasses;
        private     readonly            Dictionary<string, IWebCoreErrorHandler>                    _errorHandlers;
        private     readonly            Lock                                                        _lock;

        public                          WebLoader()
        {
            _loadedModules  = new List<Assembly>();
            _dynamicClasses = new Dictionary<WebCoreDynamicClassAttribute, ConstructorInfo>(256);
            _errorHandlers  = new Dictionary<string, IWebCoreErrorHandler>(16);
            _lock           = new Lock();

            _loadModule(typeof(WebLoader).Assembly);
        }

        public                          void                                LoadModule(string name)
        {
            _loadModule(Assembly.Load(name));
        }

        public                          WebCoreDataSource                   GetDataSource(string source, string name)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (source.Contains(Impl.Source.multiple.SplitChar, StringComparison.Ordinal)) {
                return new Impl.Source.multiple(source, name);
            }

            var sep = source.IndexOf(':', StringComparison.Ordinal);
            if (sep > 0) {
                name   = source.Substring(sep + 1);
                source = source.Substring(0, sep);
            }
            else {
                if (name == null)
                    throw new WebSourceException("Invalid source '" + source + "', name missing.");
            }

            return (WebCoreDataSource)ConstructDynamicClass(new WebCoreDataSourceAttribute(source), name);
        }
        public                          object                              ConstructDynamicClass(WebCoreDynamicClassAttribute className, params object[] args)
        {
            return ConstructDynamicClassArgs(className, args);
        }
        public                          object                              ConstructDynamicClassArgs(WebCoreDynamicClassAttribute className, object[] args)
        {
            ArgumentNullException.ThrowIfNull(className);
            ArgumentNullException.ThrowIfNull(args);

            ConstructorInfo constructorInfo = null;

            lock(_lock) {
                if (!_dynamicClasses.TryGetValue(className, out constructorInfo)) {
                    if (className.Name.IndexOf('.', StringComparison.Ordinal) > 0) {
                        for (int i = 0 ; i < _loadedModules.Count ; ++i) {
                            Type        classType = _loadedModules[i].GetType(className.Name);

                            if (classType != null) {
                                constructorInfo = className.GetConstructor(classType);
                                break;
                            }
                        }
                    }
                }
            }

            if (constructorInfo == null) {
                throw new KeyNotFoundException("Unknown " + className.ToString() + ".");
            }

            try {
                return constructorInfo.Invoke(args);
            }
            catch (System.Reflection.TargetInvocationException ex) {
                throw ex.InnerException;
            }
        }
        public                          IWebCoreErrorHandler                GetErrorHandler(string className)
        {
            IWebCoreErrorHandler    errorHandler;

            lock(_lock) {
                if (!_errorHandlers.TryGetValue(className, out errorHandler)) {
                    throw new KeyNotFoundException("Unknown error handler " + className + ".");
                }
            }

            return errorHandler;
        }

        private                         void                                _loadModule(Assembly assembly)
        {
            lock(_lock) {
                if (!_loadedModules.Contains(assembly)) {
                    _loadedModules.Add(assembly);

                    foreach(Type type in assembly.GetTypes()) {
                        try {
                            foreach(WebCoreDynamicClassAttribute attr in type.GetCustomAttributes(typeof(WebCoreDynamicClassAttribute), false)) {
                                _dynamicClasses.Add(attr, attr.GetConstructor(type));
                            }

                            if (type.GetTypeInfo().IsClass && typeof(IWebCoreErrorHandler).IsAssignableFrom(type)) {
                                _errorHandlers.Add(type.FullName, (IWebCoreErrorHandler)(type.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>())));
                            }
                        }
                        catch(Exception err) {
                            throw new WebInitializationException("Failed to process class '" + type.FullName + "'.", err);
                        }
                    }
                }
            }
        }
    }
}
