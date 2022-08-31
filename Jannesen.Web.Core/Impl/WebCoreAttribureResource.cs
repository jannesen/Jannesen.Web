using System;
using System.Reflection;

namespace Jannesen.Web.Core.Impl
{
    public sealed class WebCoreAttribureResource: WebCoreAttribureDynamicClass
    {
        public      override    string                          Type
        {
            get {
                return "resource";
            }
        }

        public                                                  WebCoreAttribureResource(string name): base(name)
        {
        }

        public      override    ConstructorInfo                 GetConstructor(Type classType)
        {
            return GetConstructorFor(classType, typeof(WebCoreResource), typeof(WebCoreConfigReader));
        }
    }
}
