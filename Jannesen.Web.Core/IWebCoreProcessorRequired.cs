using System;
using System.Collections.Generic;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessorRequired
{
    IEnumerable<IWebCoreProcessor<object>>      GetRequiredProcessors();
}
