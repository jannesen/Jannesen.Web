using System;
using System.Reflection;

namespace Jannesen.Web.Core.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WebCoreAttributeDataSource: WebCoreAttribureDynamicClass
    {
        public      override    string                          Type
        {
            get {
                return "data-source";
            }
        }

        public                                                  WebCoreAttributeDataSource(string name): base(name)
        {
        }

        public      override    ConstructorInfo                 GetConstructor(Type classType)
        {
            return GetConstructorFor(classType, typeof(WebCoreDataSource), typeof(string));
        }
    }
}
