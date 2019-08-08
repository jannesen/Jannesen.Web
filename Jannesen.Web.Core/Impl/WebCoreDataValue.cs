using System;
using System.Globalization;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl
{
    public enum WebCoreDataValueType
    {
        NoValue         = 0,
        ClrValue        = 1,
        StringValue     = 2
    }

#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct WebCoreDataValue
    {
        public          WebCoreDataValueType    Type;
        public          object                  Value;

        public          bool                    hasValue
        {
            get {
                return (Type == WebCoreDataValueType.ClrValue || Type == WebCoreDataValueType.StringValue);
            }
        }
        public          string                  StringValue
        {
            get {
                switch(Type) {
                default:
                case WebCoreDataValueType.NoValue:
                    return null;

                case WebCoreDataValueType.ClrValue:
                    if (Value is string)        return (string)Value;
                    if (Value is byte)          return ((byte) Value).ToString(CultureInfo.InvariantCulture);
                    if (Value is Int16)         return ((Int16)Value).ToString(CultureInfo.InvariantCulture);
                    if (Value is Int32)         return ((Int32)Value).ToString(CultureInfo.InvariantCulture);
                    if (Value is Int64)         return ((Int64)Value).ToString(CultureInfo.InvariantCulture);

                    throw new InvalidOperationException("No conversion possible from " + Value.GetType().FullName + " to StringValue.");

                case WebCoreDataValueType.StringValue:
                    return (string)Value;
                }
            }
        }
        public  static  WebCoreDataValue        NoValue
        {
            get {
                return new WebCoreDataValue(WebCoreDataValueType.NoValue, null);
            }
        }

        public                                  WebCoreDataValue(string sValue)
        {
            this.Type  = sValue != null ? WebCoreDataValueType.StringValue : WebCoreDataValueType.NoValue;
            this.Value = sValue;
        }
        public                                  WebCoreDataValue(object clrValue)
        {
            if (clrValue != null) {
                this.Type  = WebCoreDataValueType.ClrValue;
                this.Value = clrValue;
            }
            else {
                this.Type  = WebCoreDataValueType.NoValue;
                this.Value = null;
            }
        }
        public                                  WebCoreDataValue(WebCoreDataValueType type, object value)
        {
            this.Type  = type;
            this.Value = value;
        }
    }
#pragma warning restore CA1815
}
