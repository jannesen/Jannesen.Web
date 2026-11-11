using System;

namespace Jannesen.Web.Core.Impl;

public interface IWebCoreProcessor
{
    void                Proces(WebCoreCall httpCall);
}
