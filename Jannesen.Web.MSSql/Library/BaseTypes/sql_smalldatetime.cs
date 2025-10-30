using System;
using System.Data;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("smalldatetime")]
    internal sealed class sql_smalldatetime: sql_datetime
    {
        public          override            SqlDbType           DBType      => SqlDbType.SmallDateTime;
        public          override            Type                ClrType     => typeof(DateTime);

        public                                                  sql_smalldatetime(string s): base(s)
        {
        }

        public          override            object?             ConvertClrToValue(object? value)
        {
            if (value == null)             return null;
            if (value is DateTime)         return value;
            if (value is string   vstring) return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object?             ConvertStringToValue(string? sValue)
        {
            return ConvertStringToValue(sValue, true);
        }

        public          override            string              ToString()
        {
            return "smalldatetime";
        }
    }
}
