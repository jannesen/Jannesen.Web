using System;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("urlpath")]
    sealed class http_urlpath: WebCoreDataSource
    {
        public                                              http_urlpath(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            return new WebCoreDataValue(httpCall.RequestUrlPathData[Name]);
        }

        public      override        string                  ToString()
        {
            return "querystring:" + Name;
        }
    }
}
