﻿using System;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("querystring")]
    class http_querystring: WebCoreDataSource
    {
        public                                          http_querystring(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            return new WebCoreDataValue(httpCall.Request.QueryString[Name]);
        }

        public      override        string              ToString()
        {
            return "querystring:" + Name;
        }
    }
}
