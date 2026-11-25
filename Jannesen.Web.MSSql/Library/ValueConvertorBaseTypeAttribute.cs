using System;
using System.Reflection;
using Jannesen.Web.Core;

namespace Jannesen.Web.MSSql.Library;

public sealed class ValueConvertorBaseTypeAttribute: WebCoreDynamicClassAttribute
{
    public      override    string                          Type            => "sql-type";

    public                                                  ValueConvertorBaseTypeAttribute(string name): base(name)
    {
    }

    public      override    ConstructorInfo                 GetConstructor(Type classType)
    {
        return GetConstructorFor(classType, typeof(ValueConvertor), typeof(string));
    }
}
