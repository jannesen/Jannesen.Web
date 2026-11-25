using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Jannesen.Web.Core;

[WebCoreDataSource("querystring")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
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
