using System;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("body")]
    class http_body: WebCoreDataSource
    {
        enum ValueNameCode
        {
            ContentType,
            ContentLength,
            Data,
            Text,
            Json,
            TextXml
        }

        private                     ValueNameCode           _valueCode;

        public                                              http_body(string name_args): base(name_args)
        {
            switch(Name) {
            case "content-type":
            case "content_type":        _valueCode = ValueNameCode.ContentType;     break;
            case "content-length":
            case "content_length":      _valueCode = ValueNameCode.ContentLength;   break;
            case "data":                _valueCode = ValueNameCode.Data;            break;
            case "text":                _valueCode = ValueNameCode.Text;            break;
            case "json":                _valueCode = ValueNameCode.Json;            break;
            case "textxml":             _valueCode = ValueNameCode.TextXml;         break;

            default:                    throw new WebSourceException("Unknown source body '" + name_args + "'.");
            }
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            if (httpCall.Request.HttpMethod == "GET" || httpCall.Request.HttpMethod == "HEAD")
                throw new WebHandlerConfigException("BODY not available in with HTTP/GET.");

            switch(_valueCode) {
            case ValueNameCode.ContentType:     return new WebCoreDataValue((object)httpCall.RequestContentType);
            case ValueNameCode.ContentLength:   return new WebCoreDataValue((object)httpCall.RequestContentLength);
            case ValueNameCode.Data:            return new WebCoreDataValue((object)httpCall.GetBodyData());
            case ValueNameCode.Text:            return new WebCoreDataValue((object)httpCall.GetBodyString("text/plain"));
            case ValueNameCode.Json:            return new WebCoreDataValue((object)httpCall.GetBodyString("application/json"));
            case ValueNameCode.TextXml:         return new WebCoreDataValue((object)httpCall.GetBodyString("text/xml"));
            default:                            throw new NotImplementedException("Parameter header:" + Name + " not implemented.");
            }
        }

        public      override        string                  ToString()
        {
            return "body:" + Name;
        }
    }
}
