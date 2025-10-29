using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("smallint")]
    internal sealed class sql_smallint: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      => SqlDbType.SmallInt;
        public          override            Type                ClrType     => typeof(Int16);

        public                                                  sql_smallint(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null   )           return null;
            if (value is Int16  )           return value;
            if (value is byte   vbyte)      return Convert.ToInt16(vbyte);
            if (value is Int32  vint32)     return Convert.ToInt16(vint32);
            if (value is Int64  vint64)     return Convert.ToInt16(vint64);
            if (value is string vstring)    return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return Int16.Parse(sValue, CultureInfo.InvariantCulture);
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
