using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("real")]
    internal sealed class sql_real: ValueConvertor_SqlNativeWithLength
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Real;          } }
        public          override            Type                ClrType     { get { return typeof(float);                       } }

        public                                                  sql_real(string s): base(s)
        {
            if (Length < 1 || Length > 24)
                throw new FormatException("Syntax error sql-type.");
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is float)     return value;
            if (value is double)    return Convert.ToSingle((double)value);
            if (value is Int16)     return Convert.ToSingle((Int16)value);
            if (value is Int32)     return Convert.ToSingle((Int32)value);
            if (value is Int64)     return Convert.ToSingle((Int64)value);
            if (value is decimal)   return Convert.ToSingle((decimal)value);
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return Single.Parse(sValue, CultureInfo.InvariantCulture);
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            ConvertNumberValueToJson(sValue, jsonWriter);
        }

        public          override            string              ToString()
        {
            return "real(" + Length.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
