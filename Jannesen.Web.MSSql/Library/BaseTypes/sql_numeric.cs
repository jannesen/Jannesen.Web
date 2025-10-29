using System;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("numeric")]
    internal sealed class sql_numeric: sql_decimal
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
