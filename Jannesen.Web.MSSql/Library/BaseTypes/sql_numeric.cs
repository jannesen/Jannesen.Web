using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;


namespace Jannesen.Web.MSSql.Library.BaseTypes;

[ValueConvertorBaseType("numeric")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
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
