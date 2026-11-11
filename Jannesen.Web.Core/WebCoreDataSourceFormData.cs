using System;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core;

[WebCoreDataSourceAttribute("formdata")]
sealed class WebCoreDataSourceFormData: WebCoreDataSource
{
    public                                          WebCoreDataSourceFormData(string name_args): base(name_args)
    {
    }

    public      override        WebCoreDataValue    GetValue(WebCoreCall httpCall)
    {
        if (httpCall.Request.Method == "GET" || httpCall.Request.Method == "HEAD")
            throw new WebHandlerConfigException("FORMDATA-BODY not available for HTTP/GET.");

        return new WebCoreDataValue(httpCall.Request.Form[Name]);
    }

    public      override        string              ToString()
    {
        return "formdata:" + Name;
    }
}
