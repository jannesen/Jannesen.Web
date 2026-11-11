using System;

namespace Jannesen.Web.Core;

public interface IWebCoreDataSource
{
    WebCoreDataValue        GetValue(WebCoreCall call);
}
