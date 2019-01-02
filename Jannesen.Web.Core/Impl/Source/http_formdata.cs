using System;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("formdata")]
    class http_formdata: WebCoreDataSource
    {
        public                                          http_formdata(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue    GetValue(WebCoreCall httpCall)
        {
            if (httpCall.Request.HttpMethod == "GET" || httpCall.Request.HttpMethod == "HEAD")
                throw new WebHandlerConfigException("FORMDATA-BODY not available for HTTP/GET.");

            return new WebCoreDataValue(httpCall.Request.Form[Name]);
        }

        public      override        string              ToString()
        {
            return "formdata:" + Name;
        }
    }
}
