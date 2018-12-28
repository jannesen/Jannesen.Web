using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("datetime2")]
    class sql_datetime2: sql_datetime
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.DateTime2;     } }
        public          override            Type                ClrType     { get { return typeof(DateTime);                    } }

        public                                                  sql_datetime2(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            return base.ConvertClrToValue(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            return base.ConvertStringToValue(sValue, true);
        }

        public          override            string              ToString()
        {
            return "datetime2";
        }
    }
}
