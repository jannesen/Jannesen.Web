using System;

namespace Jannesen.Web.Core.Impl;

public interface IWebCoreCallProcessor
{
    void                Proces(WebCoreCall httpCall);
}
