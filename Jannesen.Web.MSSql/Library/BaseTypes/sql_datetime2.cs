using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("datetime2")]
    class sql_datetime2: sql_datetime
    {
        private readonly                    int                 _precision;

        public                              int                 Precision
        {
            get {
                return _precision;
            }
        }

        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.DateTime2;     } }
        public          override            Type                ClrType     { get { return typeof(DateTime);                    } }

        public                                                  sql_datetime2(string s): base(s)
        {
            if (string.IsNullOrEmpty(s))
                throw new FormatException("Syntax error sql-type datetime2.");

            _precision = int.Parse(s, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public          override            object              ConvertClrToValue(object value)
        {
            return base.ConvertClrToValue(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            return ConvertStringToValue(sValue, true);
        }

        public          override            string              ToString()
        {
            return "datetime2";
        }
    }
}
