using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseTypes;

[ValueConvertorBaseType("bigint")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
internal sealed class sql_bigint: ValueConvertor_SqlNative
{
    public          override            SqlDbType           DBType      => SqlDbType.BigInt;
    public          override            Type                ClrType     => typeof(Int64);

    public                                                  sql_bigint(string s): base(s)
    {
    }

    public          override            object?             ConvertClrToValue(object? value)
    {
        if (value == null)            return null;
        if (value is Int64)           return value;
        if (value is Int32  vint32 )  return (Int64)vint32;
        if (value is byte   vbyte  )  return (Int64)vbyte;
        if (value is Int16  vint16 )  return (Int64)vint16;
        if (value is string vstring)  return ConvertStringToValue(vstring);

        return NoConversion(value);
    }
    public          override            object?             ConvertStringToValue(string? sValue)
    {
        if (string.IsNullOrEmpty(sValue))
            return null;

        return Int64.Parse(sValue, CultureInfo.InvariantCulture);
    }
    public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
    {
        ConvertIntValueToJson(sValue, jsonWriter);
    }

    public          override            string              ToString()
    {
        return "bigint";
    }
}
