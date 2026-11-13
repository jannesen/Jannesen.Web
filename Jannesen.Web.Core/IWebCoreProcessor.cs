using System;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessor
{
    string          Name        { get; }
    object          Process(WebCoreCall httpCall);
}

