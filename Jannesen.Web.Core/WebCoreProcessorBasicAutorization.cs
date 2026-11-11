using System;

namespace Jannesen.Web.Core;

public class WebCoreProcessorBasicAutorization
{
    public static readonly  IWebCoreProcessorDefinition    Definition = new DefinitionClass();

    internal sealed class DefinitionClass: IWebCoreProcessorDefinition
    {
        public          string          Name    => "BasicAutorization";

        public          object          Create(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);
            return new WebCoreProcessorBasicAutorization(httpCall);
        }
    }

    private readonly        string?             _username;
    private readonly        string?             _passwd;

    public                  string?             UserName        => _username;
    public                  string?             Passwd          => _passwd;

    internal                                    WebCoreProcessorBasicAutorization(WebCoreCall httpCall)
    {
        var authorization = httpCall.GetHeader("Authorization");
        if (authorization != null && authorization.StartsWith("Basic ", StringComparison.Ordinal)) {
            var AuthStr     = System.Text.Encoding.ASCII.GetString(System.Convert.FromBase64String(authorization.Substring(6)));
            var AuthStrPos  = AuthStr.IndexOf(':', StringComparison.Ordinal);

            if (AuthStrPos>=0) {
                _username = AuthStr.Substring(0, AuthStrPos);
                _passwd   = AuthStr.Substring(AuthStrPos+1);
                return ;
            }
        }

        throw new WebBasicAutorizationException("Basic authorization missing");
    }
}
