using System;
using System.Collections.Generic;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core;

[WebCoreDataSourceAttribute("textjson")]
internal sealed class WebCoreDataSourceTextJson: WebCoreDataSource
{
    public                                                              WebCoreDataSourceTextJson(string name_args): base(name_args)
    {
    }

    public      override        IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        return [ WebCoreProcessorTextJson.Definition ];
    }
    public      override        WebCoreDataValue                        GetValue(WebCoreCall httpCall)
    {
        if (httpCall.Request.Method == "GET" || httpCall.Request.Method == "HEAD")
            throw new WebHandlerConfigException("TEXTJSON-BODY not available for HTTP/GET.");

        if (httpCall.GetProcessorResult(WebCoreProcessorTextJson.Definition).GetValue(Name, out var value)) {
            if (!(value == null || value is Int32 || value is Int64 || value is string || value is float || value is double || value is bool))
                throw new WebInvalidValueException("Invalid JSON value '" + value.GetType().FullName + "'.");

            return new WebCoreDataValue(value);
        }
        else
            return WebCoreDataValue.NoValue;
    }

    public      override        string                                  ToString()
    {
        return "textjson:" + Name;
    }
}
