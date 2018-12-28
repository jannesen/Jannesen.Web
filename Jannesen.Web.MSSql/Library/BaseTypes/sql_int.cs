using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("int")]
    class sql_int: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Int;           } }
        public          override            Type                ClrType     { get { return typeof(Int32);                       } }

        public                                                  sql_int(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is byte)      return Convert.ToInt32((byte)value);
            if (value is Int16)     return Convert.ToInt32((Int16)value);
            if (value is Int32)     return value;
            if (value is Int64)     return Convert.ToInt32((Int64)value);
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return Int32.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture);
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
