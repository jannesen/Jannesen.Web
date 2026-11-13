using System;

namespace Jannesen.Web.Core;

public class WebCoreProcessorBasicAutorization: IWebCoreProcessor<WebCoreProcessResultBasicAutorization>
{
    public static readonly  IWebCoreProcessor<WebCoreProcessResultBasicAutorization>    Definition = new WebCoreProcessorBasicAutorization();

    public          string                                  Name    => "BasicAutorization";

    public          WebCoreProcessResultBasicAutorization   Process(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        var authorization = httpCall.GetHeader("Authorization");
        if (authorization != null && authorization.StartsWith("Basic ", StringComparison.Ordinal)) {
            var authstr     = System.Text.Encoding.ASCII.GetString(System.Convert.FromBase64String(authorization.Substring(6)));
            var authstrPos  = authstr.IndexOf(':', StringComparison.Ordinal);

            if (authstrPos>=0) {
                return new WebCoreProcessResultBasicAutorization(
                               authstr.Substring(0, authstrPos),
                               authstr.Substring(authstrPos+1)
                           );
            }
        }

        throw new WebBasicAutorizationException("Basic authorization missing");
    }
}
