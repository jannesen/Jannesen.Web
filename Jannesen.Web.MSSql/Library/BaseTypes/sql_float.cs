using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("float")]
    class sql_float: ValueConvertor_SqlNativeWithLength
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Float;         } }
        public          override            Type                ClrType     { get { return typeof(double);                      } }

        public                                                  sql_float(string s): base(s)
        {
            if (Length < 1 || Length > 53)
                throw new FormatException("Syntax error sql-type.");
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is double)    return value;
            if (value is float)     return Convert.ToDouble((float)value);
            if (value is byte)      return Convert.ToDouble((byte)value);
            if (value is Int16)     return Convert.ToDouble((Int16)value);
            if (value is Int32)     return Convert.ToDouble((Int32)value);
            if (value is Int64)     return Convert.ToDouble((Int64)value);
            if (value is decimal)   return Convert.ToDouble((decimal)value);

            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return float.Parse(sValue, System.Globalization.CultureInfo.InvariantCulture);
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            ConvertNumberValueToJson(sValue, jsonWriter);
        }

        public          override            string              ToString()
        {
            return "float(" + Length.ToString() + ")";
        }
    }
}
