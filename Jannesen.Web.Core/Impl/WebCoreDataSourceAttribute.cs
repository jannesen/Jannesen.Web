using System;
using System.Reflection;

namespace Jannesen.Web.Core.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WebCoreDataSourceAttribute: WebCoreDynamicClassAttribute
    {
        public      override    string                          Type
        {
            get {
                return "data-source";
            }
        }

        public                                                  WebCoreDataSourceAttribute(string name): base(name)
        {
        }

        public      override    ConstructorInfo                 GetConstructor(Type classType)
        {
            return GetConstructorFor(classType, typeof(WebCoreDataSource), typeof(string));
        }
    }
}
