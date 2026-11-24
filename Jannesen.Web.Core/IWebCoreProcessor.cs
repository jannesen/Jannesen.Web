using System;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessor<out T>: IWebCoreProcessorRequired where T: class
{
    T               Process(WebCoreCall httpCall);
}
