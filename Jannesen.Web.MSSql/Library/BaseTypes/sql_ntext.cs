using System;
using System.Data;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("ntext")]
    internal sealed class sql_ntext: ValueConvertor_SqlNative
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
