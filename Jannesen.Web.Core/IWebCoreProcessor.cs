using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessor: IWebCoreProcessorRequired
{
    ValueTask<object>           ProcessAsync(WebCoreCall httpCall);
}

public interface IWebCoreProcessor<T>: IWebCoreProcessor where T: class
{
}
