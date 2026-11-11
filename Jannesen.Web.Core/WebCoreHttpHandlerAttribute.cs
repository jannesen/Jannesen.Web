using System;
using System.Reflection;

namespace Jannesen.Web.Core;

public sealed class WebCoreHttpHandlerAttribute: WebCoreDynamicClassAttribute
{
    public      override    string                          Type        => "http-handler";

    public                                                  WebCoreHttpHandlerAttribute(string name): base(name)
    {
    }

    public      override    ConstructorInfo                 GetConstructor(Type classType)
    {
        return GetConstructorFor(classType, typeof(WebCoreHttpHandler), typeof(WebCoreConfigReader));
    }
}
