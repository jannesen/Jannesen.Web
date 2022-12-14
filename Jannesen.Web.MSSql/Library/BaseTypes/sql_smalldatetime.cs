using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("smalldatetime")]
    internal sealed class sql_smalldatetime: sql_datetime
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.SmallDateTime; } }
        public          override            Type                ClrType     { get { return typeof(DateTime);                    } }

        public                                                  sql_smalldatetime(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is DateTime)  return value;
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            return ConvertStringToValue(sValue, true);
        }

        public          override            string              ToString()
        {
            return "smalldatetime";
        }
    }
}
