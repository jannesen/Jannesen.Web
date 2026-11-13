using System;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessor<T>  where T: class
{
    string          Name        { get; }
    T               Process(WebCoreCall httpCall);
}

