using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Jannesen.Web.Core;

public sealed class WebLoader
{
    public  static  readonly        WebLoader          Instance = new WebLoader();

    private     readonly            List<Assembly>                                              _loadedModules;
    private     readonly            Dictionary<WebCoreDynamicClassAttribute, ConstructorInfo>   _dynamicClasses;
    private     readonly            Dictionary<string, WebCoreErrorHandler>                     _errorHandlers;
    private     readonly            Dictionary<string, IWebCoreProcessorGetDatasource>          _dataSourcesProcessors;
    private     readonly            Dictionary<string, IWebCoreDataSource>                      _dataSources;
    private     readonly            Lock                                                        _lock;

    public                                                              WebLoader()
    {
        _loadedModules         = new List<Assembly>();
        _dynamicClasses        = new Dictionary<WebCoreDynamicClassAttribute, ConstructorInfo>(256);
        _errorHandlers         = new Dictionary<string, WebCoreErrorHandler>(16);
        _dataSourcesProcessors = new Dictionary<string, IWebCoreProcessorGetDatasource>(16);
        _dataSources           = new Dictionary<string, IWebCoreDataSource>(1024);
        _lock                  = new Lock();

    }
    internal                        void                                Init()
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

    internal                        IWebCoreDataSource                  GetDataSource(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        try {
            var reader = new DataSourceReader(source);
            IWebCoreDataSource  rtn;

            lock(_dataSources) {
                rtn = _parseMulti(reader);
            }

            reader.Next().Test(DataSourceReader.TokenType.EOF);

            return rtn;
        }
        catch(Exception ex) {
            throw new WebSourceException($"Invalid datasource 'source'.", ex);
        }
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

    private                         IWebCoreDataSource                  _parseMulti(DataSourceReader reader)
    {
        var ds = _parseSingle(reader);

        if (reader.Peek.Type == DataSourceReader.TokenType.Pipe) {
            var m = new List<IWebCoreDataSource>();
            m.Add(ds);

            while (reader.NextIf(DataSourceReader.TokenType.Pipe)) {
                m.Add(_parseSingle(reader));
            }

            ds = new WebCoreDataSourceMultiple(m.ToArray());
        }

        return ds;
    }
    private                         IWebCoreDataSource                  _parseSingle(DataSourceReader reader)
    {
        var tprocessor = reader.Next().TestName();

        if (reader.NextIf(DataSourceReader.TokenType.LParen)) {
            var args = new List<IWebCoreDataSource>();

            if (reader.Peek.Type != DataSourceReader.TokenType.RParen) {
                do {
                    args.Add(_parseMulti(reader));
                }
                while (reader.NextIf(DataSourceReader.TokenType.Comma));
            }

            reader.Next().Test(DataSourceReader.TokenType.RParen);
            var processorname = reader.Str[tprocessor.Start..reader.Peek.Start];

            reader.Next().Test(DataSourceReader.TokenType.Colon);
            var tname  = reader.Next().TestName();
            var dsname = reader.Str[tprocessor.Start..reader.Peek.Start];

            return _getDatasource(dsname, processorname, tprocessor, args, tname);
        }
        else {
            reader.Next().Test(DataSourceReader.TokenType.Colon);
            var tname  = reader.Next().TestName();
            var dsname = reader.Str[tprocessor.Start..reader.Peek.Start];

            return _getDatasource(dsname, tprocessor, tname);
        }
    }
    private                         IWebCoreDataSource                  _getDatasource(string dsname, DataSourceReader.Token tprocessor, DataSourceReader.Token tname)
    {
        if (!_dataSources.TryGetValue(dsname, out var datasource)) {
            datasource = (IWebCoreDataSource)ConstructDynamicClass(new WebCoreDataSourceAttribute(tprocessor.GetString()), tname.GetString());
            _dataSources[dsname] = datasource;
        }

        return datasource;
    }
    private                         IWebCoreDataSource                  _getDatasource(string dsname, string processorname, DataSourceReader.Token tprocessor, List<IWebCoreDataSource> args,  DataSourceReader.Token tname)
    {
        if (!_dataSourcesProcessors.TryGetValue(processorname, out var processor)) {
            var argsType = new Type[args.Count];
            for (var i = 0; i < argsType.Length ; i++) {
                argsType[i] = args[i].GetType();
            }

            var sprocessor    = tprocessor.GetString();
            var processorType = _getTypeByClassFullName(sprocessor);
            if (!typeof(IWebCoreProcessorGetDatasource).IsAssignableFrom(processorType)) {
                throw new FormatException($"Class {sprocessor} not implements IWebCoreProcessor.");
            }

            processor = (IWebCoreProcessorGetDatasource)(processorType.GetConstructor(argsType) ?? throw new FormatException($"Can't instance {sprocessor}, invalid number of arguments.")).Invoke(args.ToArray());
            _dataSourcesProcessors[processorname] = processor;
            System.Diagnostics.Debug.WriteLine($"Create processor: {processorname}");
        }


        if (!_dataSources.TryGetValue(dsname, out var datasource)) {
            datasource = processor.GetDataSource(tname.GetString());
            _dataSources[dsname] = datasource;
        }

        return datasource;
    }
}
