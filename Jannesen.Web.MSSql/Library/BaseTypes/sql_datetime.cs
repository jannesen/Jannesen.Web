using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("datetime")]
    class sql_datetime: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.DateTime;      } }
        public          override            Type                ClrType     { get { return typeof(DateTime);                    } }

        public                                                  sql_datetime(string s): base(s)
        {
        }

        public          override            object              ConvertStringToValue(string sValue)
        {
            return ConvertStringToValue(sValue, false);
        }
        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is DateTime)  return value;
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }

        protected                           object              ConvertStringToValue(string sValue, bool small)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            int     fieldpos = 0;
            int[]   fields   = new int[7];
            int     factor   = 0;

            for (int pos = 0 ; pos<sValue.Length ; ++pos) {
                char    chr = sValue[pos];

                if (chr>='0' && chr <='9') {
                    if (fieldpos<6)
                        fields[fieldpos] = fields[fieldpos]*10 + (chr-'0');
                    else {
                        fields[fieldpos] += (chr-'0')*factor;
                        factor /= 10;
                    }
                }
                else {
                    switch(fieldpos) {
                    case 0: // year
                    case 1: // month
                        if (chr!='-') goto invalid_date;
                        break;

                    case 2: // day
                        if (chr!='T') goto invalid_date;
                        break;

                    case 3: // hour
                    case 4: // minute
                        if (chr!=':') goto invalid_date;
                        break;

                    case 5: // second
                        if (chr!='.') goto invalid_date;
                        factor = 100;
                        break;

                    default:
invalid_date:               throw new System.FormatException("Invalid date format.");
                    }

                    ++fieldpos;
                }
            }

            if (fields[1]<1 || fields[1]>12 ||
                fields[2]<1 || fields[2]>31 ||
                fields[3]>23 ||
                fields[4]>59 ||
                fields[5]>59)
                throw new FormatException("Invalid date format.");

            if (small) {
                if (fields[0]<1753 || fields[0]>2079 ||
                    (fields[0]==2079 && fields[1]>5))
                    throw new System.FormatException("smalldatetime out of range.");
            }
            else {
                if (fields[0]<1900 || fields[0]>2999)
                    throw new System.FormatException("datetime out of range.");
            }

            return new DateTime(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], fields[6]);
        }

        public          override            string              ToString()
        {
            return "datetime";
        }
    }
}
