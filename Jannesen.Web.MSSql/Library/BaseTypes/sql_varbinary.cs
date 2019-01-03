using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("varbinary")]
    class sql_varbinary: ValueConvertor_SqlNativeWithLength
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.VarBinary;     } }
        public          override            Type                ClrType     { get { return typeof(byte[]);                      } }

        public                                                  sql_varbinary(string s): base(s)
        {
            if ((Length < 1 || Length > 8000) && Length != int.MaxValue)
                throw new FormatException("Syntax error sql-type.");
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is byte[] byteValue) {
                if (byteValue.Length > Length)
                    throw new FormatException("Varbinary longer then " + Length.ToString() + " .");

                return byteValue;
            }
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (sValue == null)
                return null;

            byte[]  bValue = System.Convert.FromBase64String(sValue);

            if (bValue.Length > Length)
                throw new FormatException("Varbinary longer then " + Length.ToString() + " .");

            return bValue;
        }

        public          override            string              ToString()
        {
            return "varbinary(" + Length.ToString() + ")";
        }
    }
}
