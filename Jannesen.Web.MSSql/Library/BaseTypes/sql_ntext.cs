using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("ntext")]
    class sql_ntext: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.NText;         } }
        public          override            Type                ClrType     { get { return typeof(string);                      } }

        public                                                  sql_ntext(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            return sValue;
        }

        public          override            string              ToString()
        {
            return "ntext";
        }
    }
}
