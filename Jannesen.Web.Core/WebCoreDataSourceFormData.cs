using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Jannesen.Web.Core;

[WebCoreDataSource("formdata")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
sealed class WebCoreDataSourceFormData: WebCoreDataSource
{
    public                                                              WebCoreDataSourceFormData(string name_args): base(name_args)
    {
    }

    public      override        IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        return [];
    }
    public      override        WebCoreDataValue                        GetValue(WebCoreCall httpCall)
    {
        if (httpCall.Request.Method == "GET" || httpCall.Request.Method == "HEAD")
            throw new WebHandlerConfigException("FORMDATA-BODY not available for HTTP/GET.");

        return new WebCoreDataValue(httpCall.Request.Form[Name]); //!!TODO Request.Form is loads de body.
    }

    public      override        string                                  ToString()
    {
        return "formdata:" + Name;
    }
}
