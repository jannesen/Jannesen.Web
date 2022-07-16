using System;
using System.Reflection;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.MSSql.Library
{
    public sealed class ValueConvertorAttributeBaseType: WebCoreAttribureDynamicClass
    {
        public      override    string                          Type
        {
            get {
                return "sql-type";
            }
        }

        public                                                  ValueConvertorAttributeBaseType(string name): base(name)
        {
        }

        public      override    ConstructorInfo                 GetConstructor(Type classType)
        {
            return GetConstructorFor(classType, typeof(ValueConvertor), typeof(string));
        }
    }
}
