using System;
using System.Collections.Generic;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core;

[WebCoreDataSourceAttribute("urlpath")]
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
