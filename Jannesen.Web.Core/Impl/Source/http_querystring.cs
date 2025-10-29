using System;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreDataSourceAttribute("querystring")]
    sealed class http_querystring: WebCoreDataSource
    {
        public                                          http_querystring(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue    GetValue(WebCoreCall httpCall)
        {
            return new WebCoreDataValue(httpCall.Request.Query[Name]);
        }

        public      override        string              ToString()
        {
            return "querystring:" + Name;
        }
    }
}
