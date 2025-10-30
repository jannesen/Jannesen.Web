using System;
using System.Globalization;

namespace Jannesen.Web.Core.Impl
{
    public enum WebCoreDataValueType
    {
        NoValue         = 0,
        ClrValue        = 1,
        StringValue     = 2
    }

#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types
    public readonly struct WebCoreDataValue
    {
        public          WebCoreDataValueType    Type        { get; private init; }
        public          object?                 Value       { get; private init; }

        public          bool                    hasValue
        {
            get {
                return (Type == WebCoreDataValueType.ClrValue || Type == WebCoreDataValueType.StringValue);
            }
        }
        public          string?                 StringValue
        {
            get {
                switch(Type) {
                default:
                case WebCoreDataValueType.NoValue:
                    return null;

                case WebCoreDataValueType.ClrValue:
                    if (Value is string vstring) return vstring;
                    if (Value is byte   vbyte)   return vbyte.ToString(CultureInfo.InvariantCulture);
                    if (Value is Int16  vint16)  return vint16.ToString(CultureInfo.InvariantCulture);
                    if (Value is Int32  vint32)  return vint32.ToString(CultureInfo.InvariantCulture);
                    if (Value is Int64  vint64)  return vint64.ToString(CultureInfo.InvariantCulture);

                    throw new InvalidOperationException("No conversion possible from " + (Value != null ? Value.GetType().FullName : "[null]") + " to StringValue.");

                case WebCoreDataValueType.StringValue:
                    return (string?)Value;
                }
            }
        }
        public  static  WebCoreDataValue        NoValue
        {
            get {
                return new WebCoreDataValue(WebCoreDataValueType.NoValue, null);
            }
        }

        public                                  WebCoreDataValue(string? sValue)
        {
            this.Type  = sValue != null ? WebCoreDataValueType.StringValue : WebCoreDataValueType.NoValue;
            this.Value = sValue;
        }
        public                                  WebCoreDataValue(object? clrValue)
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
        public                                  WebCoreDataValue(WebCoreDataValueType type, object? value)
        {
            this.Type  = type;
            this.Value = value;
        }
    }
#pragma warning restore CA1815
}
