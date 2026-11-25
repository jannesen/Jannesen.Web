using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Jannesen.Web.MSSql.Library.BaseTypes;

[ValueConvertorBaseType("text")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
internal sealed class sql_text: ValueConvertor_SqlNative
{
    public          override            SqlDbType           DBType      => SqlDbType.Text;
    public          override            Type                ClrType     => typeof(string);

    public                                                  sql_text(string s): base(s)
    {
    }

    public          override            object?             ConvertClrToValue(object? value)
    {
        if (value == null)      return null;
        if (value is string)    return value;

        return NoConversion(value);
    }
    public          override            object?             ConvertStringToValue(string? sValue)
    {
        return sValue;
    }

    public          override            string              ToString()
    {
        return "text";
    }
}
