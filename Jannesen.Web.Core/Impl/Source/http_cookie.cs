using System;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("cookie")]
    sealed class http_cookie: WebCoreDataSource
    {
        public                                          http_cookie(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            return new WebCoreDataValue(httpCall.Request.Cookies[Name]);
        }

        public      override        string              ToString()
        {
            return "cookie:" + Name;
        }
    }
}
