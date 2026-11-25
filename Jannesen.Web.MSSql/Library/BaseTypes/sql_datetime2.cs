using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseTypes;

[ValueConvertorBaseType("datetime2")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
internal sealed class sql_datetime2: ValueConvertor_SqlNative
{
    private readonly                    int                 _precision;

    public                              int                 Precision
    {
        get {
            return _precision;
        }
    }

    public          override            SqlDbType           DBType      => SqlDbType.DateTime2;
    public          override            Type                ClrType     => typeof(DateTime);

    public                                                  sql_datetime2(string s)
    {
        if (string.IsNullOrEmpty(s))
            throw new FormatException("Syntax error sql-type datetime2.");

        _precision = int.Parse(s, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    public          override            object?             ConvertStringToValue(string? sValue)
    {
        return sql_datetime.ConvertStringToValue(sValue, false);
    }
    public          override            object?             ConvertClrToValue(object? value)
    {
        if (value == null)             return null;
        if (value is DateTime)         return value;
        if (value is string   vstring) return ConvertStringToValue(vstring);

        return NoConversion(value);
    }

    public          override            string              ToString()
    {
        return "datetime2";
    }
}
