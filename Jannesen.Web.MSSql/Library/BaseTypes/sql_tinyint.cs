using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("tinyint")]
    class sql_tinyint: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.TinyInt;       } }
        public          override            Type                ClrType     { get { return typeof(byte);                        } }

        public                                                  sql_tinyint(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is byte)      return (byte)value;
            if (value is Int16)     return Convert.ToByte((Int16)value);
            if (value is Int32)     return Convert.ToByte((Int32)value);
            if (value is Int64)     return Convert.ToByte((Int64)value);
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return byte.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture);
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
