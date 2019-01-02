using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("image")]
    class sql_image: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Image;         } }
        public          override            Type                ClrType     { get { return typeof(byte[]);                      } }

        public                                                  sql_image(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is byte[])    return value;
            if (value is string)    return ConvertStringToValue((string)value);

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
