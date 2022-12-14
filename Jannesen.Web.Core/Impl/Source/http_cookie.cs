using System;
using System.Web;
using Jannesen.Web.Core.Impl;

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
            HttpCookieCollection    cookies = httpCall.Request.Cookies;
            string[]                keys    = cookies.AllKeys;
            string                  value   = null;

            for (int i = 0 ; i < keys.Length ; ++i ) {
                if (keys[i] == Name) {
                    value = cookies[i].Value;
                    break;
                }
            }

            return new WebCoreDataValue(value);
        }

        public      override        string              ToString()
        {
            return "cookie:" + Name;
        }
    }
}
