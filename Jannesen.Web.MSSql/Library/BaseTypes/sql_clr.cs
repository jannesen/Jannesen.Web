using System;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Jannesen.Web.MSSql.Library.BaseType
{
    [ValueConvertorAttributeBaseType("clr")]
    class sql_clr: ValueConvertor_SqlNative
    {
        private                             Type                _type;
        private                             int                 _size;
        private                             MethodInfo          _parse;

        public          override            SqlDbType           DBType      { get { return System.Data.SqlDbType.VarBinary;     } }
        public          override            Type                ClrType     { get { return typeof(byte[]);                      } }

        public                                                  sql_clr(string s)
        {
            var parts = s.Split(':');

            if (parts.Length != 2)
                throw new FormatException("Syntax error sql-type.");

            _type = Assembly.Load(parts[0]).GetType(parts[1], true);

            var sqlUserAttr = _type.GetCustomAttribute<Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute>();

            if (sqlUserAttr != null && sqlUserAttr.Format == Microsoft.SqlServer.Server.Format.Native && sqlUserAttr.IsByteOrdered && sqlUserAttr.IsFixedLength && _type.IsValueType) {
                _size = Marshal.SizeOf(_type);
                if (_size < 1 || _size > 8000)
                    throw new InvalidOperationException("Not a support type.");
            }
            else
                throw new NotSupportedException("Not a support type '" + s + "'.");

            _parse = _type.GetMethod("Parse", new Type[] { typeof(System.Data.SqlTypes.SqlString) });

            if (!(_parse != null && _parse.IsStatic))
                throw new InvalidOperationException("Missing parse method.");
        }

        public          override            object              ConvertClrToValue(object value)
        {
            if (value == null)      return null;
            if (value is string)    return ConvertStringToValue((string)value);

            return NoConversion(value);
        }
        public          override            object              ConvertStringToValue(string sValue)
        {
            if (string.IsNullOrEmpty(sValue))
                return null;

            return _toByteArray(_parse.Invoke(null, new object[] { new System.Data.SqlTypes.SqlString(sValue) }));
        }
        public          override            void                ConvertXmlValueToJson(string sValue, Jannesen.FileFormat.Json.JsonWriter jsonWriter)
        {
            if (string.IsNullOrEmpty(sValue)) {
                jsonWriter.WriteNull();
                return;
            }

            var binarydata = Convert.FromBase64String(sValue);

            if (binarydata.Length != _size)
                throw new FormatException("Invalid binary data for " + _type.Name);

            jsonWriter.WriteString(_fromByteArray(binarydata).ToString());
        }

        public          override            string              ToString()
        {
            return "clr";
        }

        private                             byte[]              _toByteArray(object structdata)
        {
            if (structdata.GetType() != _type)
                throw new InvalidOperationException("Invalid type");

            var rtn = new byte[_size];

            GCHandle h = GCHandle.Alloc(rtn, GCHandleType.Pinned);

            try {
                Marshal.StructureToPtr(structdata, h.AddrOfPinnedObject(), false);
            }
            finally {
                h.Free();
            }

            return rtn;
        }
        private                             object              _fromByteArray(byte[] binarydata)
        {
            GCHandle h = GCHandle.Alloc(binarydata, GCHandleType.Pinned);

            try {
                return Marshal.PtrToStructure(h.AddrOfPinnedObject(), _type);
            }
            finally {
                h.Free();
            }
        }
    }
}
