using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jannesen.Web.Core
{
    public static class WebExtensions
    {
        public  static      void                AddJannesenWeb(this IWebHostBuilder webHostBuilder, Func<WebApplication> createAppl)
        {
            ArgumentNullException.ThrowIfNull(webHostBuilder);

            webHostBuilder.ConfigureServices((services) => {
                services.AddSingleton(createAppl());
                services.AddMemoryCache();
                services.Configure<IISServerOptions>((options) => {
                            options.AllowSynchronousIO = true;
                        });
            });

            webHostBuilder.Configure((appConfig) => {
                var webAppl = appConfig.ApplicationServices.GetRequiredService<WebApplication>();
                webAppl.Initialize(appConfig.ApplicationServices);
                appConfig.Use((next) => (context) => webAppl.HttpHandler(context, next));
            });
        }
    }
}
