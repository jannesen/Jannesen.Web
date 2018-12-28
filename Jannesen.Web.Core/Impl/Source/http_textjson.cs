using System;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("textjson")]
    class http_textjson: WebCoreDataSource
    {
        public                                              http_textjson(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            if (httpCall.Request.HttpMethod == "GET" || httpCall.Request.HttpMethod == "HEAD")
                throw new WebHandlerConfigException("TEXTJSON-BODY not available for HTTP/GET.");

            if (httpCall.RequestTextJson.GetValue(Name, out var value)) {
                if (!(value == null || value is Int32 || value is Int64 || value is string || value is float || value is double || value is bool))
                    throw new WebInvalidValueException("Invalid JSON value '" + value.GetType().FullName + "'.");

                return new WebCoreDataValue(value);
            }
            else
                return WebCoreDataValue.NoValue;
        }

        public      override        string                  ToString()
        {
            return "textjson:" + Name;
        }
    }
}
