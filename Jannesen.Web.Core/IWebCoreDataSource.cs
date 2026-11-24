using System;

namespace Jannesen.Web.Core;

public interface IWebCoreDataSource: IWebCoreProcessorRequired
{
    WebCoreDataValue        GetValue(WebCoreCall call);
}
