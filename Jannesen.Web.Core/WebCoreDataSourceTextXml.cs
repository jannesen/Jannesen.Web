using System;
using System.Collections.Generic;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core;

[WebCoreDataSourceAttribute("textxml")]
internal sealed class WebCoreDataSourceTextXml: WebCoreDataSource
{
    public                                                              WebCoreDataSourceTextXml(string name_args): base(name_args)
    {
    }

    public      override        IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        return [ WebCoreProcessorTextXml.Definition ];
    }
    public      override        WebCoreDataValue                        GetValue(WebCoreCall httpCall)
    {
        if (httpCall.Request.Method == "GET" || httpCall.Request.Method == "HEAD")
            throw new WebHandlerConfigException("TEXTXML-BODY not available for HTTP/GET.");

        return new WebCoreDataValue(httpCall.GetProcessorResult(WebCoreProcessorTextXml.Definition).GetStringValue(Name));
    }

    public      override        string                                  ToString()
    {
        return "textxml:" + Name;
    }
}
