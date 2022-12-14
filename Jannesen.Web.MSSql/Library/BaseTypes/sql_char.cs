using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("char")]
    internal sealed class sql_char: ValueConvertor_SqlNativeWithLength
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.VarChar;       } }
        public          override            Type                ClrType     { get { return typeof(string);                      } }

        public                                                  sql_char(string s): base(s)
        {
            if (Length < 1 || Length > 8000)
                throw new FormatException("Syntax error sql-type.");
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (sValue == null)
                return null;

            if (sValue.Length > Length)
                throw new FormatException("String longer then " + Length.ToString(CultureInfo.InvariantCulture) + " .");

            return sValue;
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            jsonWriter.WriteValue(sValue.TrimEnd());
        }


        public          override            string              ToString()
        {
            return "varchar(" + Length.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
