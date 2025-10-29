using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("int")]
    internal sealed class sql_int: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      => SqlDbType.Int;
        public          override            Type                ClrType     => typeof(Int32);

        public                                                  sql_int(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)           return null;
            if (value is Int32)          return value;
            if (value is byte   vbyte)   return Convert.ToInt32(vbyte);
            if (value is Int16  vint16)  return Convert.ToInt32(vint16);
            if (value is Int64  vint64)  return Convert.ToInt32(vint64);
            if (value is string vstring) return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return Int32.Parse(sValue, CultureInfo.InvariantCulture);
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            ConvertIntValueToJson(sValue, jsonWriter);
        }

        public          override            string              ToString()
        {
            return "int";
        }
    }
}
