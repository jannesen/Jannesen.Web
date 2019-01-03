using System;
using System.Reflection;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WebCoreAttributeDataSource: WebCoreAttribureDynamicClass
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
