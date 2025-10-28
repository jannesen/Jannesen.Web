using System;
using System.Threading;
using System.Threading.Tasks;
using Jannesen.Web.Core.Impl;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

//!!TODO Use refcount on _applicationConfig

namespace Jannesen.Web.Core
{
    public sealed class WebApplication: IDisposable
    {
        public                          string                          Directory           {  get; init; }
        public                          string                          Realm               {  get; init; }

        private                         IServiceProvider                                            _serviceProvider;
        private                         IMemoryCache                                                _cache;             
        private                         WebApplicationConfig                                        _applicationConfig;
        private                         Lock                                                        _configLock;

        public                          IMemoryCache                                                Cache => _cache;

        public                          WebApplication()
        {
            Realm              = "website";
            _applicationConfig = null;
            _configLock        = new Lock();
        }
        public      void                Initialize(IServiceProvider serviceProvider)
        {
            lock(_configLock) {
                if (_serviceProvider != null) {
                    throw new InvalidOperationException("WebMainService already initialized");
                }

                _serviceProvider = serviceProvider;
                _cache           = serviceProvider.GetRequiredService<IMemoryCache>();
                _loadApplicationConfig();
            }
        }

        public      void                Dispose()
        {
            lock(_configLock) {
                _applicationConfig?.Dispose();
                _applicationConfig = null;
                _cache?.Dispose();
                _cache = null;
            }
        }

        public                          Task                                HttpHandler(HttpContext context, RequestDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            var applConfig = _getApplicationConfig();

            var h = applConfig.GetHttpHandler(context.Request.Path, context.Request.Method);

            if (h != null) {
                return Task.Run(() => h.ProcessRequest(applConfig, context));
            }

            return next(context);
        }

        public                          void                                LogInfo(string message)
        {
            Console.WriteLine("inf: " + message);
        }
        public                          void                                LogWarning(string message)
        {
            Console.WriteLine("wrn: " + message);
        }
        public                          void                                LogError(string message, Exception exception=null)
        {
            while (exception != null) {
                var exmsg = exception.Message;
                if (exception is IFileLocation fileLocation) {
                    exmsg += " (location=" + fileLocation.Filename + ":" + fileLocation.LineNumber + ")";
                }

                message = (message != null) ? message + " " + exmsg : exmsg;
                exception = exception.InnerException;
            }

            Console.WriteLine("err: " + message);
        }

        private                         WebApplicationConfig                _getApplicationConfig()
        {
            lock(_configLock) {
                if (_applicationConfig.NeedsReInitialize()) {
                    _loadApplicationConfig();
                }

                return _applicationConfig;
            }
        }
        private                         void                                _loadApplicationConfig()
        {
            var applicationConfig = new WebApplicationConfig(this);
            applicationConfig.Load(_serviceProvider, Directory);

            _applicationConfig?.Dispose();
            _applicationConfig = applicationConfig;
        }
    }
}
