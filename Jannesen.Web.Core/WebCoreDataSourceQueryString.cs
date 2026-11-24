using System;
using System.Collections.Generic;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core;

[WebCoreDataSourceAttribute("querystring")]
internal sealed class WebCoreDataSourceQueryString: WebCoreDataSource
{
    public                                                              WebCoreDataSourceQueryString(string name_args): base(name_args)
    {
    }

    public      override        IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        return [];
    }
    public      override        WebCoreDataValue                        GetValue(WebCoreCall httpCall)
    {
        return new WebCoreDataValue(httpCall.Request.Query[Name]);
    }

    public      override        string                                  ToString()
    {
        return "querystring:" + Name;
    }
}
