using System;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessor: IWebCoreProcessorRequired
{
    object              Process(WebCoreCall httpCall);
}

public interface IWebCoreProcessor<T>: IWebCoreProcessor where T: class
{
}
