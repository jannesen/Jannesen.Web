using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("nvarchar")]
    internal sealed class sql_nvarchar: ValueConvertor_SqlNativeWithLength
    {
        public          override            SqlDbType           DBType      => SqlDbType.NVarChar;
        public          override            Type                ClrType     => typeof(string);

        public                                                  sql_nvarchar(string s): base(s)
        {
            if ((Length < 1 || Length > 4000) && Length != int.MaxValue)
                throw new FormatException("Syntax error sql-type.");
        }

        public          override            object?             ConvertClrToValue(object? value)
        {
            if (value == null)           return null;
            if (value is string vstring) return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object?             ConvertStringToValue(string? sValue)
        {
            if (sValue == null)
                return null;

            if (sValue.Length > Length)
                throw new FormatException("String longer then " + Length.ToString(CultureInfo.InvariantCulture) + " .");

            return sValue;
        }

        public          override            string              ToString()
        {
            return "varchar(" + Length.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
