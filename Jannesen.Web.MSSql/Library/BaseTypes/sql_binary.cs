using System;
using System.Data;
using System.Globalization;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("binary")]
    internal sealed class sql_binary: ValueConvertor_SqlNativeWithLength
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Binary;        } }
        public          override            Type                ClrType     { get { return typeof(byte[]);                      } }

        public                                                  sql_binary(string s): base(s)
        {
            if (Length < 1 || Length > 8000)
                throw new FormatException("Syntax error sql-type.");
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null) {
                return null;
            }

            if (value is byte[] byteValue) {
                if (byteValue.Length != Length)
                    throw new FormatException("Invalid length of binary.");

                return value;
            }

            if (value is string vstring) {
                return ConvertStringToValue(vstring);
            }

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (sValue == null)
                return null;

            var bValue = System.Convert.FromBase64String(sValue);

            if (bValue.Length != Length)
                throw new FormatException("Invalid length of binary.");

            return bValue;
        }

        public          override            string              ToString()
        {
            return "binary(" + Length.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
