using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("varbinary")]
    internal sealed class sql_varbinary: ValueConvertor_SqlNativeWithLength
    {
        public          override            SqlDbType           DBType      => SqlDbType.VarBinary;
        public          override            Type                ClrType     => typeof(byte[]);

        public                                                  sql_varbinary(string s): base(s)
        {
            if ((Length < 1 || Length > 8000) && Length != int.MaxValue)
                throw new FormatException("Syntax error sql-type.");
        }

        public          override            object?             ConvertClrToValue(object? value)
        {
            if (value == null) {
                return null;
            }

            if (value is byte[] byteValue) {
                if (byteValue.Length > Length)
                    throw new FormatException("Varbinary longer then " + Length.ToString(CultureInfo.InvariantCulture) + " .");

                return value;
            }

            if (value is string vstring) {
                return ConvertStringToValue(vstring);
            }

            return NoConversion(value);
        }
        public          override            object?             ConvertStringToValue(string? sValue)
        {
            if (sValue == null)
                return null;

            var bValue = System.Convert.FromBase64String(sValue);

            if (bValue.Length > Length)
                throw new FormatException("Varbinary longer then " + Length.ToString(CultureInfo.InvariantCulture) + " .");

            return bValue;
        }

        public          override            string              ToString()
        {
            return "varbinary(" + Length.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
