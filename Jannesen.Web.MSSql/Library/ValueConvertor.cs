using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using Jannesen.Web.Core;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.MSSql.Library
{
    public abstract class ValueConvertor
    {
        public      abstract            SqlDbType           DBType                      { get; }
        public      abstract            Type                ClrType                     { get; }

        private     static readonly     Dictionary<string, ValueConvertor>       _cache = new Dictionary<string, ValueConvertor>(256);

        public      static              ValueConvertor      GetType(string nameparameter)
        {
            ValueConvertor   valueType;

            lock(_cache) {
                if (!_cache.TryGetValue(nameparameter, out valueType)) {
                    string  name;
                    string  parm;
                    int     b = nameparameter.IndexOf('(');

                    if (b > 0) {
                        int     e = nameparameter.IndexOf(')', b + 1);

                        if (e != nameparameter.Length - 1)
                            throw new FormatException("Syntax error sql-type.");

                        name = nameparameter.Substring(0,   b);
                        parm = nameparameter.Substring(b+1, e - (b+1));
                    }
                    else {
                        name = nameparameter;
                        parm = null;
                    }

                    valueType = (ValueConvertor)WebApplication.ConstructDynamicClass(new ValueConvertorAttributeBaseType(name), parm);
                    _cache.Add(nameparameter, valueType);
                }
            }

            return valueType;
        }

        public      virtual             object              ConvertToSqlParameterValue(WebCoreDataValue value)
        {
            switch (value.Type) {
            case WebCoreDataValueType.ClrValue:     return ConvertClrToValue(value.Value);
            case WebCoreDataValueType.StringValue:  return ConvertStringToValue((string)(value.Value));
            default:                                return null;
            }
        }

        public      abstract            object              ConvertClrToValue(object value);
        public      abstract            object              ConvertStringToValue(string sValue);
        public      virtual             void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            jsonWriter.WriteValue(sValue);
        }

        protected                       object              NoConversion(object value)
        {
            throw new WebConversionException("No convertion from " + value.GetType().FullName + " sql-datatype " + ToString() + ".");
        }
    }

    public abstract class ValueConvertor_SqlNative: ValueConvertor
    {
        protected                                           ValueConvertor_SqlNative()
        {
        }
        protected                                           ValueConvertor_SqlNative(string s)
        {
            if (s != null)
                throw new FormatException("Syntax error type.");
        }

        protected   static              void                ConvertIntValueToJson(string sint, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            if (string.IsNullOrEmpty(sint))
                jsonWriter.WriteNull();
            else
                jsonWriter.WriteRawValue(sint);
        }
        protected   static              void                ConvertNumberValueToJson(string snumber, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            if (string.IsNullOrEmpty(snumber))
                jsonWriter.WriteNull();
            else
                jsonWriter.WriteRawValue(_normalizeNumberValueToJson(snumber));
        }

        private     static              string              _normalizeNumberValueToJson(string snumber)
        {
            var  rtn = new StringBuilder(snumber.Length);
            int  length = snumber.Length;
            int  p    = 0;

            // Trim +
            if (snumber[p] == '+')
                ++p;

            // Copy - to rtn
            if (snumber[p] == '-')
                rtn.Append(snumber[p++]);

            // Skip zeros
            while (p < length && snumber[p] == '0')
                ++p;

            // Copy digits
            if (p < length && ('0' <= snumber[p] && snumber[p] <= '9')) {
                do
                    rtn.Append(snumber[p++]);
                while (p < length && ('0' <= snumber[p] && snumber[p] <= '9'));
            }
            else
                rtn.Append('0');

            // Has fraction
            if (p < length && snumber[p] == '.') {
                int     b = p;      // Begin fraction
                int     e = p-1;    // Last digit

                p++;

                while (p < length && ('0' <= snumber[p] && snumber[p] <= '9')) {
                    if (snumber[p] != '0')
                        e = p;

                    p++;
                }

                // Copy fraction
                while (b <= e)
                    rtn.Append(snumber[b++]);
            }

            if (p < length && (snumber[p] == 'e' || snumber[p] == 'E')) {
                char    sign = ' ';
                int     e = 0;
                ++p;

                if (p < length && (snumber[p] == '+' || snumber[p] == '-'))
                    sign = snumber[p++];

                while (p < length && ('0' <= snumber[p] && snumber[p] <= '9'))
                    e = (e * 10) + (snumber[p++] - '0');

                if (e != 0) {
                    rtn.Append('e');
                    rtn.Append(sign == '0' ? -e : e);
                }
            }

            if (p<length)
                throw new FormatException("Invalid string number.");

            return rtn.ToString();
        }
    }

    public abstract class ValueConvertor_SqlNativeWithLength: ValueConvertor_SqlNative
    {
        private readonly                int                 _length;

        public                          int                 Length
        {
            get {
                return _length;
            }
        }

        protected                                           ValueConvertor_SqlNativeWithLength(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new FormatException("Syntax error sql-type.");

            _length = (s == "max") ? int.MaxValue : int.Parse(s, System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
    }
}
