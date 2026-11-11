using System;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessor
{
    void                Proces(WebCoreCall httpCall);
}
