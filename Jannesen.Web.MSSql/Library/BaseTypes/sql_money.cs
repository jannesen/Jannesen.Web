using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("money")]
    class sql_money: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Money;         } }
        public          override            Type                ClrType     { get { return typeof(decimal);                     } }

        public                                                  sql_money(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is decimal)   return value;
            if (value is float)     return Convert.ToDecimal((float)value);
            if (value is double)    return Convert.ToDecimal((double)value);
            if (value is Int16)     return Convert.ToDecimal((Int16)value);
            if (value is Int32)     return Convert.ToDecimal((Int32)value);
            if (value is Int64)     return Convert.ToDecimal((Int64)value);
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
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
