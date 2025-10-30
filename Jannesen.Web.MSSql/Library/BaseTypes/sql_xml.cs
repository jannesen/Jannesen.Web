using System;
using System.Data;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.BaseTypes
{
    [ValueConvertorAttributeBaseType("xml")]
    internal sealed class sql_xml: ValueConvertor_SqlNative
    {
        public          override            SqlDbType           DBType      => SqlDbType.Xml;
        public          override            Type                ClrType     => typeof(string);

        public                                                  sql_xml(string s): base(s)
        {
        }

        public          override            object?             ConvertClrToValue(object? value)
        {
            if (value == null)           return null;
            if (value is string vstring) return ConvertStringToValue(vstring);

            return NoConversion(value);
        }
        public          override            object?             ConvertStringToValue(string? sValue)
        {
            return sValue;
        }

        public          override            string              ToString()
        {
            return "xml";
        }
    }
}
