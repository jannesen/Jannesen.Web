using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("smallmoney")]
    internal sealed class sql_smallmoney: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      => SqlDbType.SmallMoney;
        public          override            Type                ClrType     => typeof(decimal);

        public                                                  sql_smallmoney(string s): base(s)
        {
        }

        public          override            object?             ConvertClrToValue(object? value)
        {
            if (value == null)              return null;
            if (value is decimal)           return value;
            if (value is double vdouble)    return Convert.ToDecimal(vdouble);
            if (value is float  vfloat)     return Convert.ToDecimal(vfloat);
            if (value is byte   vbyte)      return Convert.ToDecimal(vbyte);
            if (value is Int16  vint16)     return Convert.ToDecimal(vint16);
            if (value is Int32  vint32)     return Convert.ToDecimal(vint32);
            if (value is Int64  vint64)     return Convert.ToDecimal(vint64);
            if (value is string vstring)    return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object?             ConvertStringToValue(string? sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return decimal.Parse(sValue, CultureInfo.InvariantCulture);
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            ConvertNumberValueToJson(sValue, jsonWriter);
        }

        public          override            string              ToString()
        {
            return "money";
        }
    }
}
