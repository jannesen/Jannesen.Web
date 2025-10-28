using System;
using System.Reflection;

namespace Jannesen.Web.Core.Impl
{
    public sealed class WebCoreResourceAttribute: WebCoreDynamicClassAttribute
    {
        public      override    string                          Type
        {
            get {
                return "resource";
            }
        }

        public                                                  WebCoreResourceAttribute(string name): base(name)
        {
        }

        public      override    ConstructorInfo                 GetConstructor(Type classType)
        {
            return GetConstructorFor(classType, typeof(WebCoreResource), typeof(WebCoreConfigReader));
        }
    }
}
