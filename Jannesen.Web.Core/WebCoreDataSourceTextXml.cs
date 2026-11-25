using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Jannesen.Web.Core;

[WebCoreDataSource("textxml")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
internal sealed class WebCoreDataSourceTextXml: WebCoreDataSource
{
    public                                                          WebCoreDataSourceTextXml(string name_args): base(name_args)
    {
    }

    public      override        IEnumerable<IWebCoreProcessor>      GetRequiredProcessors()
    {
        return [ WebCoreProcessorTextXml.Processor ];
    }
    public      override        WebCoreDataValue                    GetValue(WebCoreCall httpCall)
    {
        if (httpCall.Request.Method == "GET" || httpCall.Request.Method == "HEAD")
            throw new WebHandlerConfigException("TEXTXML-BODY not available for HTTP/GET.");

        return new WebCoreDataValue(httpCall.GetProcessorResult(WebCoreProcessorTextXml.Processor).GetStringValue(Name));
    }

    public      override        string                              ToString()
    {
        return "textxml:" + Name;
    }
}
