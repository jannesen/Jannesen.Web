using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("decimal")]
    class sql_decimal: ValueConvertor_SqlNative
    {
        private readonly                    int                 _precision;
        private readonly                    int                 _scale;

        public                              int                 Precision
        {
            get {
                return _precision;
            }
        }
        public                              int                 Scale
        {
            get {
                return _scale;
            }
        }

        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Decimal;       } }
        public          override            Type                ClrType     { get { return typeof(decimal);                     } }

        public                                                  sql_decimal(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new FormatException("Syntax error sql-type.");

            try {
                int     i = s.IndexOf(',');

                if (i < 0) {
                    _precision = int.Parse(s, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
                    _scale     = 0;
                }
                else {
                    _precision = int.Parse(s.Substring(0, i), System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
                    _scale     = int.Parse(s.Substring(i + 1), System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                throw new FormatException("Syntax error sql-type.");
            }
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is decimal)   return value;
            if (value is float)     return Convert.ToDecimal((float)value);
            if (value is double)    return Convert.ToDecimal((double)value);
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
            return "decimal(" + Precision.ToString(CultureInfo.InvariantCulture) + "." + Scale.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
