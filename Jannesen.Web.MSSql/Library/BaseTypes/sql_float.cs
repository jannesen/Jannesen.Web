using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("float")]
    internal sealed class sql_float: ValueConvertor_SqlNativeWithLength
    {
        public          override            SqlDbType           DBType      => SqlDbType.Float;
        public          override            Type                ClrType     => typeof(double);

        public                                                  sql_float(string s): base(s)
        {
            if (Length < 1 || Length > 53)
                throw new FormatException("Syntax error sql-type.");
        }

        public          override            object?             ConvertClrToValue(object? value)
        {
            if (value == null)             return null;
            if (value is double)           return value;
            if (value is float   vfloat)   return Convert.ToDouble(vfloat);
            if (value is Int32   vint32)   return Convert.ToDouble(vint32);
            if (value is byte    vbyte)    return Convert.ToDouble(vbyte);
            if (value is Int16   vint16)   return Convert.ToDouble(vint16);
            if (value is Int64   vint64)   return Convert.ToDouble(vint64);
            if (value is decimal vdecimal) return Convert.ToDouble(vdecimal);
            if (value is string  vstring)  return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object?             ConvertStringToValue(string? sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return float.Parse(sValue, CultureInfo.InvariantCulture);
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            ConvertNumberValueToJson(sValue, jsonWriter);
        }

        public          override            string              ToString()
        {
            return "float(" + Length.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
