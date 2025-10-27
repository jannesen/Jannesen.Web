using System;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("querystring")]
    sealed class http_querystring: WebCoreDataSource
    {
        public                                          http_querystring(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            return new WebCoreDataValue(httpCall.Request.Query[Name]);
        }

        public      override        string              ToString()
        {
            return "querystring:" + Name;
        }
    }
}
