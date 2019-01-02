using System;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("textxml")]
    class http_textxml: WebCoreDataSource
    {
        public                                              http_textxml(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            if (httpCall.Request.HttpMethod == "GET" || httpCall.Request.HttpMethod == "HEAD")
                throw new WebHandlerConfigException("TEXTXML-BODY not available for HTTP/GET.");

            return new WebCoreDataValue(httpCall.RequestTextXml.GetStringValue(Name));
        }

        public      override        string                  ToString()
        {
            return "textxml:" + Name;
        }
    }
}
