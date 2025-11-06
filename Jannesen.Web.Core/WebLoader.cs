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
        private     readonly            Dictionary<string, WebCoreErrorHandler>                     _errorHandlers;
        private     readonly            Lock                                                        _lock;

        public                          WebLoader()
        {
            _loadedModules  = new List<Assembly>();
            _dynamicClasses = new Dictionary<WebCoreDynamicClassAttribute, ConstructorInfo>(256);
            _errorHandlers  = new Dictionary<string, WebCoreErrorHandler>(16);
            _lock           = new Lock();

        }
        public                          void                                Init()
        {
            // Load a
            _loadAssembly(this.GetType().Assembly);
            _loadAssembly(Assembly.GetEntryAssembly());
            // make sure standard is loaded.
            GetErrorHandler(null);
        }
        public                          void                                LoadModule(string name)
        {
            _loadAssembly(Assembly.Load(name));
        }

        public                          WebCoreDataSource                   GetDataSource(string source, string? name)
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
        public                          object                              ConstructDynamicClass(WebCoreDynamicClassAttribute className, params object?[] args)
        {
            return ConstructDynamicClassArgs(className, args);
        }
        public                          object                              ConstructDynamicClassArgs(WebCoreDynamicClassAttribute className, object?[] args)
        {
            ArgumentNullException.ThrowIfNull(className);
            ArgumentNullException.ThrowIfNull(args);

            ConstructorInfo? constructorInfo;

            lock(_lock) {
                if (!_dynamicClasses.TryGetValue(className, out constructorInfo)) {
                    if (className.Name.IndexOf('.', StringComparison.Ordinal) > 0) {
                        constructorInfo = className.GetConstructor(_getTypeByClassFullName(className.Name));
                        _dynamicClasses.Add(className, constructorInfo);
                    }
                }
            }

            if (constructorInfo == null) {
                throw new KeyNotFoundException("Unknown " + className.ToString() + ".");
            }

            try {
                return constructorInfo.Invoke(args);
            }
            catch (TargetInvocationException ex) {
                if (ex.InnerException != null) {
                    throw ex.InnerException;
                }
                throw;
            }
        }
        public                          WebCoreErrorHandler                 GetErrorHandler(string? handlerClassName)
        {
            if (handlerClassName == null) handlerClassName = typeof(WebCoreStdErrorHandler).FullName!;
            WebCoreErrorHandler?  errorHandler;

            lock(_lock) {
                if (!_errorHandlers.TryGetValue(handlerClassName, out errorHandler)) {
                    var createMethod = _getTypeByClassFullName(handlerClassName)
                                       .GetMethod("Create", BindingFlags.Static|BindingFlags.Public, [ typeof(WebCoreHttpHandler), typeof(Exception) ]) ??
                                            throw new InvalidOperationException("Error handler class '" + handlerClassName + "' has nog static Create.");

                    try {
                        errorHandler = (WebCoreErrorHandler)Delegate.CreateDelegate(typeof(WebCoreErrorHandler), createMethod);
                    }
                    catch(Exception err) {
                        throw new InvalidOperationException("Can't create delegate for handler class '" + handlerClassName + ".Create()'.", err);
                    }

                    _errorHandlers.Add(handlerClassName, errorHandler);
                }
            }

            return errorHandler;
        }

        private                         void                                _loadAssembly(Assembly? assembly)
        {
            if (assembly != null) {
                lock(_lock) {
                    if (!_loadedModules.Contains(assembly)) {
                        _loadedModules.Add(assembly);

                        foreach(var type in assembly.GetTypes()) {
                            if (type != null && type.FullName != null) {
                                try {
                                    foreach(var attr in (WebCoreDynamicClassAttribute[])type.GetCustomAttributes(typeof(WebCoreDynamicClassAttribute), false)) {
                                        _dynamicClasses.Add(attr, attr.GetConstructor(type));
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
        private                         Type                                _getTypeByClassFullName(string name)
        {
            for (var i = 0 ; i < _loadedModules.Count ; ++i) {
                var classType = _loadedModules[i].GetType(name);

                if (classType != null) {
                    return classType;
                }
            }

            throw new KeyNotFoundException("Unknown class " + name + ".");
        }
    }
}
