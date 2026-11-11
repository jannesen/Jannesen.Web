using System;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessorDefinition
{
    string          Name        { get; }
    object          Create(WebCoreCall httpCall);
}

