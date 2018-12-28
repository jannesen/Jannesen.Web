using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Jannesen.Web.Core.Impl
{
    public sealed class WebCoreConfigReader : IDisposable
    {
        private                 WebApplication      _application;
        private                 string              _path;
        private                 string              _filename;
        private                 XmlTextReader       _xmlReader;

        public                  string              Path
        {
            get {
                return _path;
            }
        }
        public                  WebApplication      Application
        {
            get {
                return _application;
            }
        }
        public                  string              Filename
        {
            get {
                return _filename;
            }
        }
        public                  int                 LineNumber
        {
            get {
                return _xmlReader.LineNumber;
            }
        }
        public                  string              ElementName
        {
            get {
                return _xmlReader.Name;
            }
        }
        public                  bool                hasChildren
        {
            get {
                if (_xmlReader.NodeType != XmlNodeType.Element)
                    throw new InvalidOperationException("Invalid node type.");

                return !_xmlReader.IsEmptyElement;
            }
        }
        public                  bool                isElement
        {
            get {
                return (_xmlReader.NodeType == XmlNodeType.Element);
            }
        }

        public                                      WebCoreConfigReader(WebApplication application, string path, string filename)
        {
            _application = application;
            _path        = path;
            _filename    = filename;
            _xmlReader   = new XmlTextReader(_filename);
        }
        public                  void                Dispose()
        {
            _xmlReader.Close();
        }

        public                  void                ReadRootNode(string rootElementName)
        {
            do {
                if (!_xmlReader.Read())
                    throw new WebConfigException("Unexpected EOF.", this);
            }
            while (_xmlReader.NodeType != XmlNodeType.Element);

            if (_xmlReader.Name != rootElementName)
                throw new WebConfigException("Invalid root element name.", this);
        }
        public                  bool                ReadNextElement()
        {
            for(;;) {
                if (!_xmlReader.Read())
                    throw new WebConfigException("Unexpected EOF.", this);

                switch(_xmlReader.NodeType)
                {
                case XmlNodeType.Element:
                    return true;

                case XmlNodeType.EndElement:
                    return false;
                }
            }
        }
        public                  void                NoChildElements()
        {
            if (!_xmlReader.IsEmptyElement) {
                if (ReadNextElement())
                    throw new WebConfigException("Element is not empty.", this);
            }
        }
        public                  void                InvalidElement()
        {
            throw new WebConfigException("Unknown configuration entry '" + _xmlReader.Name + "'.", this);
        }
        public                  void                ReadEOF()
        {
            if (_xmlReader.NodeType != XmlNodeType.EndElement)
                throw new WebConfigException("Expect root end element.", this);

            while (_xmlReader.Read()) {
                if (_xmlReader.NodeType == XmlNodeType.Element || _xmlReader.NodeType == XmlNodeType.EndElement)
                    throw new WebConfigException("Unexpected node.", this);
            }
        }
        public                  void                Skip()
        {
            if (hasChildren) {
                while (ReadNextElement()) {
                    if (hasChildren)
                        Skip();
                }
            }
        }

        public                  string              CombinePhysicalPath(string path)
        {
            if (System.IO.Path.IsPathRooted(path))
                return path;

            StringBuilder   rtn = new StringBuilder();
            List<string>    parts;

            if (_filename.Length > 2 && _filename[1] == ':' && _filename[2] == '\\') {
                rtn.Append(_filename.Substring(0,2));
                parts = new List<string>(_filename.Substring(3).Split('\\'));
                parts.RemoveAt(parts.Count - 1);
            }
            else
            if (_filename.StartsWith("\\")) {
                throw new WebConfigException("Relative to UNC not supported.", this);
            }
            else
                throw new WebConfigException("Current file is not absolute", this);

            foreach(string p in path.Replace("\\", "/").Split('/')) {
                switch(p)
                {
                case ".":
                    break;

                case "..":
                    if (parts.Count < 1)
                        throw new WebConfigException("Invalid relative path", this);

                    parts.RemoveAt(parts.Count - 1);
                    break;

                default:
                    parts.Add(p);
                    break;
                }
            }

            foreach(string p in parts) {
                rtn.Append("\\");
                rtn.Append(p);
            }

            return rtn.ToString();
        }
        public                  string              GetValueString(string name)
        {
            string      value = _xmlReader.GetAttribute(name);

            if (value == null)
                throw new WebConfigException("Missing attribute '" + name + "'.", this);

            return value;
        }
        public                  string              GetValueString(string name, string defaultValue)
        {
            string      value = _xmlReader.GetAttribute(name);

            if (value == null)
                return defaultValue;

            return value;
        }
        public                  int                 GetValueInt(string name, int minValue, int maxValue)
        {
            string      value = _xmlReader.GetAttribute(name);
            int         ivalue;

            if (value == null)
                throw new WebConfigException("Missing attribute '" + name + "'.", this);

            try {
                ivalue = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new WebConfigException("Invalid integer value in attribute '" + name + "'.", this);
            }

            if (minValue > ivalue || ivalue > maxValue)
                throw new WebConfigException("Value in attribute '" + name + "' out of range.", this);

            return ivalue;
        }
        public                  int                 GetValueInt(string name, int defaultValue, int minValue, int maxValue)
        {
            string      value = _xmlReader.GetAttribute(name);
            int         ivalue;

            if (value == null)
                return defaultValue;

            try {
                ivalue = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new WebConfigException("Invalid integer value in attribute '" + name + "'.", this);
            }

            if (minValue > ivalue || ivalue > maxValue)
                throw new WebConfigException("Value in attribute '" + name + "' out of range.", this);

            return ivalue;
        }
        public                  int?                GetValueIntNull(string name, int minValue, int maxValue)
        {
            string      value = _xmlReader.GetAttribute(name);
            int         ivalue;

            if (value == null)
                return null;

            try {
                ivalue = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new WebConfigException("Invalid integer value in attribute '" + name + "'.", this);
            }

            if (minValue > ivalue || ivalue > maxValue)
                throw new WebConfigException("Value in attribute '" + name + "' out of range.", this);

            return ivalue;
        }
        public                  bool                GetValueBool(string name, bool defaultValue)
        {
            string      value = _xmlReader.GetAttribute(name);

            if (value == null)
                return defaultValue;

            switch(value)
            {
            case "0":
            case "n":
            case "false":
                return false;

            case "1":
            case "y":
            case "true":
                return true;

            default:
                throw new WebConfigException("Invalid boolean value in attribute '" + name + "'.", this);
            }
        }
        public                  int                 GetValueEnum(Type t, string name)
        {
            string      value = GetValueString(name);

            try {
                return (int)Enum.Parse(t, value, true);
            }
            catch(Exception) {
                throw new WebConfigException("Invalid integer value in attribute '" + name + "'.", this);
            }
        }
        public                  double              GetValueDouble(string name, double minValue, double maxValue)
        {
            string      value = _xmlReader.GetAttribute(name);
            double      dvalue;

            if (value == null)
                throw new WebConfigException("Missing attribute '" + name + "'.", this);

            try {
                dvalue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new WebConfigException("Invalid double value in attribute '" + name + "'.", this);
            }

            if (minValue > dvalue || dvalue > maxValue)
                throw new WebConfigException("Value in attribute '" + name + "' out of range.", this);

            return dvalue;
        }
        public                  double              GetValueDouble(string name, double defaultValue, double minValue, double maxValue)
        {
            string      value = _xmlReader.GetAttribute(name);
            double      dvalue;

            if (value == null)
                return defaultValue;

            try {
                dvalue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new WebConfigException("Invalid double value in attribute '" + name + "'.", this);
            }

            if (minValue > dvalue || dvalue > maxValue)
                throw new WebConfigException("Value in attribute '" + name + "' out of range.", this);

            return dvalue;
        }
        public                  double?             GetValueDoubleNull(string name, double minValue, double maxValue)
        {
            string      value = _xmlReader.GetAttribute(name);
            double      dvalue;

            if (value == null)
                return null;

            try {
                dvalue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch(Exception) {
                throw new WebConfigException("Invalid double value in attribute '" + name + "'.", this);
            }

            if (minValue > dvalue || dvalue > maxValue)
                throw new WebConfigException("Value in attribute '" + name + "' out of range.", this);

            return dvalue;
        }
        public                  string              GetValuePathName(string name)
        {
            if (_path == null)
                throw new WebConfigException("Path unavailable.", this);

            string  pathname = GetValueString(name);

//          if (pathname == "/" || pathname.IndexOf('\\') >= 0 || pathname.IndexOf("/./") >= 0 || pathname.IndexOf("/../") >= 0)
//              throw new WebConfigException("Invalid pathname in attribute '" + name + "'.", this);

            if (pathname.StartsWith("/"))
                return pathname;

            return _path + pathname;
        }
    }
}
