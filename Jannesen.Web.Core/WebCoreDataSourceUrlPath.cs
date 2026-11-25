using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Jannesen.Web.Core;

[WebCoreDataSource("urlpath")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
internal sealed class WebCoreDataSourceUrlPath: WebCoreDataSource
{
    public                                                              WebCoreDataSourceUrlPath(string name_args): base(name_args)
    {
    }

    public      override        IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        return [ WebCoreProcessorUrlPathData.Definition ];
    }
    public      override        WebCoreDataValue                        GetValue(WebCoreCall httpCall)
    {
        return new WebCoreDataValue(httpCall.GetProcessorResult(WebCoreProcessorUrlPathData.Definition)[Name]);
    }

    public      override        string                                  ToString()
    {
        return "querystring:" + Name;
    }
}
