using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("bigint")]
    internal sealed class sql_bigint: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.BigInt;        } }
        public          override            Type                ClrType     { get { return typeof(Int64);                       } }

        public                                                  sql_bigint(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is Int64)     return value;
            if (value is byte)      return (Int64)(byte)value;
            if (value is Int16)     return (Int64)(Int16)value;
            if (value is int  )     return (Int64)(int)value;
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
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
}
