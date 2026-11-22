using System;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessor<T> where T: class
{
    T                       Process(WebCoreCall httpCall);
}
