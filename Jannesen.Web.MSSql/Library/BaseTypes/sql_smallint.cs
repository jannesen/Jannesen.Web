using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("smallint")]
    class sql_smallint: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.SmallInt;      } }
        public          override            Type                ClrType     { get { return typeof(Int16);                       } }

        public                                                  sql_smallint(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is Int16)     return value;
            if (value is byte)      return Convert.ToInt16((byte)value);
            if (value is Int32)     return Convert.ToInt16((Int32)value);
            if (value is Int64)     return Convert.ToInt16((Int64)value);
            if (value is int)       return Convert.ToInt16((int)value);
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return Int16.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture);
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            ConvertIntValueToJson(sValue, jsonWriter);
        }

        public          override            string              ToString()
        {
            return "smallint";
        }
    }
}
