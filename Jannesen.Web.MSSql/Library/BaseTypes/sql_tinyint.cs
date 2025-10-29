using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("tinyint")]
    internal sealed class sql_tinyint: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.TinyInt;       } }
        public          override            Type                ClrType     { get { return typeof(byte);                        } }

        public                                                  sql_tinyint(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)           return null;
            if (value is byte)           return value;
            if (value is Int32  vint32)  return Convert.ToByte(vint32);
            if (value is Int16  vint16)  return Convert.ToByte(vint16);
            if (value is Int64  vint64)  return Convert.ToByte(vint64);
            if (value is string vstring) return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return byte.Parse(sValue, CultureInfo.InvariantCulture);
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            ConvertIntValueToJson(sValue, jsonWriter);
        }

        public          override            string              ToString()
        {
            return "tinyint";
        }
    }
}
