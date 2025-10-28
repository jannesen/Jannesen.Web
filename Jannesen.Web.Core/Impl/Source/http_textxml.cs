using System;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreDataSourceAttribute("textxml")]
    sealed class http_textxml: WebCoreDataSource
    {
        public                                              http_textxml(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            if (httpCall.Request.Method == "GET" || httpCall.Request.Method == "HEAD")
                throw new WebHandlerConfigException("TEXTXML-BODY not available for HTTP/GET.");

            return new WebCoreDataValue(httpCall.RequestTextXml.GetStringValue(Name));
        }

        public      override        string                  ToString()
        {
            return "textxml:" + Name;
        }
    }
}
