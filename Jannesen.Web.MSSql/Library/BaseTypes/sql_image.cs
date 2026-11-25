using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;


namespace Jannesen.Web.MSSql.Library.BaseTypes;

[ValueConvertorBaseType("image")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
internal sealed class sql_image: ValueConvertor_SqlNative
{
    public          override            SqlDbType           DBType      => SqlDbType.Image;
    public          override            Type                ClrType     => typeof(byte[]);

    public                                                  sql_image(string s): base(s)
    {
    }

    public          override            object?             ConvertClrToValue(object? value)
    {
        if (value == null)           return null;
        if (value is byte[])         return value;
        if (value is string vstring) return ConvertStringToValue(vstring);

        return NoConversion(value);
    }
    public          override            object?             ConvertStringToValue(string? sValue)
    {
        return !string.IsNullOrEmpty(sValue) ? Convert.FromBase64String(sValue) : null;
    }

    public          override            string              ToString()
    {
        return "image";
    }
}
