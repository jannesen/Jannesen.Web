using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseTypes;

[ValueConvertorBaseType("char")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
internal sealed class sql_char: ValueConvertor_SqlNativeWithLength
{
    public          override            SqlDbType           DBType      => SqlDbType.VarChar;
    public          override            Type                ClrType     => typeof(string);

    public                                                  sql_char(string s): base(s)
    {
        if (Length < 1 || Length > 8000)
            throw new FormatException("Syntax error sql-type.");
    }

    public          override            object?             ConvertClrToValue(object? value)
    {
        if (value == null)           return null;
        if (value is string vstring) return ConvertStringToValue(vstring);

        return NoConversion(value);
    }
    public          override            object?             ConvertStringToValue(string? sValue)
    {
        if (sValue == null)
            return null;

        if (sValue.Length > Length)
            throw new FormatException("String longer then " + Length.ToString(CultureInfo.InvariantCulture) + " .");

        return sValue;
    }
    public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
    {
        jsonWriter.WriteValue(sValue?.TrimEnd());
    }


    public          override            string              ToString()
    {
        return "varchar(" + Length.ToString(CultureInfo.InvariantCulture) + ")";
    }
}
