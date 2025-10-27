using System;
using System.Collections.Generic;
using System.Reflection;
using Jannesen.Web.Core.Impl;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core
{
    public sealed class WebLoader
    {
        public  static  readonly        WebLoader          Instance = new WebLoader();

        private     readonly            List<Assembly>                                              _loadedModules;
        private     readonly            Dictionary<WebCoreAttribureDynamicClass, ConstructorInfo>   _dynamicClasses;
        private     readonly            Dictionary<string, IWebCoreErrorHandler>                    _errorHandlers;

        public                          WebLoader()
        {
            _loadedModules     = new List<Assembly>();
            _dynamicClasses    = new Dictionary<WebCoreAttribureDynamicClass, ConstructorInfo>(256);
            _errorHandlers     = new Dictionary<string, IWebCoreErrorHandler>(16);

            _loadModule(typeof(WebLoader).Assembly);
        }

        public                          void                                LoadModule(string name)
        {
            _loadModule(Assembly.Load(name));
        }

        public                          WebCoreDataSource                   GetDataSource(string source, string name)
        {
            if (source.IndexOf(Impl.Source.multiple.SplitChar) >= 0) {
                return new Impl.Source.multiple(source, name);
            }

            var sep = source.IndexOf(":", StringComparison.Ordinal);
            if (sep > 0) {
                name   = source.Substring(sep + 1);
                source = source.Substring(0, sep);
            }
            else {
                if (name == null)
                    throw new WebSourceException("Invalid source '" + source + "', name missing.");
            }

            return (WebCoreDataSource)ConstructDynamicClass(new WebCoreAttributeDataSource(source), name);
        }
        public                          object                              ConstructDynamicClass(WebCoreAttribureDynamicClass className, params object[] args)
        {
            return ConstructDynamicClassArgs(className, args);
        }
        public                          object                              ConstructDynamicClassArgs(WebCoreAttribureDynamicClass className, object[] args)
        {
            ConstructorInfo constructorInfo = null;

            lock(this) {
                if (!_dynamicClasses.TryGetValue(className, out constructorInfo)) {
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

            lock(this) {
                if (!_errorHandlers.TryGetValue(className, out errorHandler)) {
                    throw new KeyNotFoundException("Unknown error handler " + className + ".");
                }
            }

            return errorHandler;
        }

        private                         void                                _loadModule(Assembly assembly)
        {
            lock(this) {
                if (!_loadedModules.Contains(assembly)) {
                    _loadedModules.Add(assembly);

                    foreach(Type type in assembly.GetTypes()) {
                        try {
                            foreach(WebCoreAttribureDynamicClass attr in type.GetCustomAttributes(typeof(WebCoreAttribureDynamicClass), false)) {
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
