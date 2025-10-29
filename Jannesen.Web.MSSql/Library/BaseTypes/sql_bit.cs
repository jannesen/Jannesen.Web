using System;
using System.Data;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("bit")]
    internal sealed class sql_bit: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Bit;           } }
        public          override            Type                ClrType     { get { return typeof(bool);                        } }

        public                                                  sql_bit(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)            return null;
            if (value is bool)            return value;
            if (value is Int32  vint32 )  return (vint32 != 0);
            if (value is byte   vbyte  )  return (vbyte  != 0);
            if (value is Int16  vint16 )  return (vint16 != 0);
            if (value is Int64  vint64 )  return (vint64 != 0);
            if (value is string vstring)  return ConvertStringToValue(vstring);

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
