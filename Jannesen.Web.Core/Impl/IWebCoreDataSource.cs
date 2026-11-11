using System;

namespace Jannesen.Web.Core.Impl;

public interface IWebCoreDataSource
{
    WebCoreDataValue        GetValue(WebCoreCall call);
}
