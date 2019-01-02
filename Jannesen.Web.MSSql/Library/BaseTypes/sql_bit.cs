using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("bit")]
    class sql_bit: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Bit;           } }
        public          override            Type                ClrType     { get { return typeof(bool);                        } }

        public                                                  sql_bit(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is byte)      return ((byte)value != 0);
            if (value is Int16)     return ((Int16)value != 0);
            if (value is int)       return ((int)value != 0);
            if (value is Int64)     return ((Int64)value != 0);
            if (value is bool)      return value;
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            switch(sValue) {
            case null:      return null;
            case "":        return null;
            case "1":       return true;
            case "true":    return true;
            case "0":       return false;
            case "false":   return false;
            default:        throw new FormatException("Invalid boolean value '" + sValue + "' .");
            }
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            jsonWriter.WriteValue(ConvertStringToValue(sValue));
        }

        public          override            string              ToString()
        {
            return "bit";
        }
    }
}
