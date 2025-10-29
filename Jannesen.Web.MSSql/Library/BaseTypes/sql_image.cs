using System;
using System.Data;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("image")]
    internal sealed class sql_image: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      => SqlDbType.Image;
        public          override            Type                ClrType     => typeof(byte[]);

        public                                                  sql_image(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)           return null;
            if (value is byte[])         return value;
            if (value is string vstring) return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            return System.Convert.FromBase64String(sValue);
        }

        public          override            string              ToString()
        {
            return "image";
        }
    }
}
