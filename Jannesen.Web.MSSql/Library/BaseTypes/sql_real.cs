using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
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
            if (value == null)             return null;
            if (value is float)            return value;
            if (value is double  vdouble)  return Convert.ToSingle(vdouble);
            if (value is decimal vdecimal) return Convert.ToSingle(vdecimal);
            if (value is Int32   vint32)   return Convert.ToSingle(vint32);
            if (value is byte    vbyte)    return Convert.ToSingle(vbyte);
            if (value is Int16   vint16)   return Convert.ToSingle(vint16);
            if (value is Int64   vint64)   return Convert.ToSingle(vint64);
            if (value is string  vstring)  return ConvertStringToValue(vstring);

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
