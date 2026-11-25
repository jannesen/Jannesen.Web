using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Jannesen.Web.Core;

[WebCoreDataSource("cookie")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
sealed class WebCoreDataSourceCookie: WebCoreDataSource
{
    public                                                              WebCoreDataSourceCookie(string name_args): base(name_args)
    {
    }

    public      override        IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        return [];
    }
    public      override        WebCoreDataValue                        GetValue(WebCoreCall httpCall)
    {
        return new WebCoreDataValue(httpCall.Request.Cookies[Name]);
    }

    public      override        string                                  ToString()
    {
        return "cookie:" + Name;
    }
}
