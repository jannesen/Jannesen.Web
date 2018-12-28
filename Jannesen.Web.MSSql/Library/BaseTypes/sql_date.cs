using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("date")]
    class sql_date: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.Date;          } }
        public          override            Type                ClrType     { get { return typeof(DateTime);                    } }

        public                                                  sql_date(string s): base(s)
        {
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is DateTime)  return value;
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            if (string.IsNullOrEmpty(sValue))
                return null;

            int     fieldpos = 0;
            int[]   fields   = new int[3];

            for (int pos = 0 ; pos<sValue.Length ; ++pos) {
                char    chr = sValue[pos];

                if (chr>='0' && chr <='9') {
                    fields[fieldpos] = fields[fieldpos]*10 + (chr-'0');
                }
                else {
                    switch(fieldpos)
                    {
                    case 0: // year
                    case 1: // month
                        if (chr!='-') goto invalid_date;
                        break;

                    default:
invalid_date:               throw new System.FormatException("Invalid date format.");
                    }

                    ++fieldpos;
                }
            }

            if (fields[1]<1 || fields[1]>12 ||
                fields[2]<1 || fields[2]>31)
                throw new FormatException("Invalid date format.");

            if (fields[0]<1900 || fields[0]>2999)
                throw new FormatException("date out of range.");

            return new DateTime(fields[0], fields[1], fields[2]);
        }

        public          override            string              ToString()
        {
            return "date";
        }
    }
}
