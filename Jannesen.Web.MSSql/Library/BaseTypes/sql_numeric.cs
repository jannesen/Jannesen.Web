using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("numeric")]
    class sql_numeric: sql_decimal
    {
        public                                                  sql_numeric(string s): base(s)
        {
        }

        public          override            string              ToString()
        {
            return "decimal(" + Precision.ToString(CultureInfo.InvariantCulture) + "." + Scale.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
