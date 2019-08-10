using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Jannesen.FileFormat.Json;
using Jannesen.Web.Core.Impl;
using Jannesen.Web.MSSql.Library;

namespace Jannesen.Web.MSSql.Sqx
{
    [WebCoreAttribureHttpHandler("sql-json")]
    public class HttpHandlerSqlXmlJson: HttpHandlerMSSql
    {
        public      override    string                      Mimetype
        {
            get {
                return "application/json";
            }
        }

        public                                              HttpHandlerSqlXmlJson(WebCoreConfigReader configReader): base(configReader)
        {
            if (configReader.hasChildren) {
                while (configReader.ReadNextElement()) {
                    switch(configReader.ElementName) {
                    case    "parameter":    ParseParameter(configReader);               break;
                    default:                configReader.InvalidElement();              break;
                    }
                }
            }
        }

        protected   override    WebCoreResponse             Process(WebCoreCall httpCall, SqlDataReader dataReader)
        {
            WebCoreResponseBuffer   webResponseBuffer = new WebCoreResponseBuffer("application/json; charset=utf-8", this.Public, true);

            if (HandleResponseOptions(webResponseBuffer, dataReader) == HttpStatusCode.OK) {
                object  json;

                try {
                    json = _xmlToJson(dataReader);

                    if (MapTo200) {
                        json = new JsonObject() { {"data", json } };
                    }
                }
                catch(Exception err) {
                    throw new WebResponseException("Convertion from XML to JSON failed.", err);
                }

                using (MemoryStream buffer = new MemoryStream(0x10000)) {
                    using (JsonWriter jsonWriter = new JsonWriter(new StreamWriter(buffer, new System.Text.UTF8Encoding(false), 1024, true), true))
                        jsonWriter.WriteValue(json);

                    webResponseBuffer.SetData(buffer);
                }
            }

            return webResponseBuffer;
        }

        private                 object                      _xmlToJson(SqlDataReader dataReader)
        {
            using (MemoryStream     memoryStream = new MemoryStream()) {
                bool        fempty = true;

                using (StreamWriter textStream  = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, true)) {
                    do {
                        while(dataReader.Read()) {
                            if (!dataReader.IsDBNull(0)) {
                                textStream.Write(dataReader.GetString(0));
                                fempty = false;
                            }
                        }
                    }
                    while (dataReader.NextResult());
                }

                if (!fempty) {
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var xmlReader = new XmlTextReader(new StreamReader(memoryStream, System.Text.Encoding.UTF8, false, 4096, true)){ DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }) {
                        while (xmlReader.NodeType != XmlNodeType.Element)
                            _parseReadNode(xmlReader);

                        if (xmlReader.Name=="json-array") {
                            return _parseToJsonArray(xmlReader);
                        }
                        else
                        if (xmlReader.Name.Length >= 3 && xmlReader.Name[0] == '_' && xmlReader.Name[2] == '_') {
                            switch(xmlReader.Name[1]) {
                            case 'a':       return _parseToJsonArray(xmlReader);
                            case 'o':       return _parseToJsonObject(xmlReader);
                            default:        return _jsonConvertValue(xmlReader.Name[1], _parseElementValue(xmlReader));
                            }
                        }
                        else {
                            return _parseToJsonObject(xmlReader);
                        }
                    }
                }
                else {
                    return (Verb != "GET" ? "OK" : null);
                }
            }
        }
        private                 JsonObject                  _parseToJsonObject(XmlTextReader xmlReader)
        {
            JsonObject  jsonObject = new JsonObject();

            if (xmlReader.MoveToFirstAttribute()) {
                do {
                    if (string.IsNullOrEmpty(xmlReader.NamespaceURI)) {
                        string name   = xmlReader.Name;
                        string svalue = xmlReader.Value;

                        if (name.Length > 3 && name[0] == '_' && name[2] == '_') {
                            try {
                                jsonObject.Add(name.Substring(3), _jsonConvertValue(name[1], svalue));
                            }
                            catch(Exception err) {
                                throw new WebConversionException("Invalid attribute value '" + name + "' value='" + svalue + "'.", err);
                            }
                        }
                        else
                            jsonObject.Add(name, svalue);
                    }
                }
                while (xmlReader.MoveToNextAttribute());

                xmlReader.MoveToElement();
            }

            if (xmlReader.IsEmptyElement)
                return jsonObject;

            for (;;) {
                _parseReadNode(xmlReader);

                switch(xmlReader.NodeType) {
                case XmlNodeType.EndElement:
                    return jsonObject;

                case XmlNodeType.Element: {
                        string  name = xmlReader.Name;

                        try {
                            if (name.Length > 3 && name[0] == '_' && name[2] == '_') {
                                switch(name[1]) {
                                case 'a':       jsonObject.Add(name.Substring(3), _parseToJsonArray(xmlReader));                                    break;
                                case 'o':       jsonObject.Add(name.Substring(3), _parseToJsonObject(xmlReader));                                   break;
                                default:        jsonObject.Add(name.Substring(3), _jsonConvertValue(name[1], _parseElementValue(xmlReader)));       break;
                                }
                            }
                            else {
                                if (jsonObject.TryGetValue(name, out var obj)) {
                                    if (!(obj is JsonArray))
                                        throw new WebConversionException("Variable already defined as a non-array.");
                                }
                                else
                                    jsonObject.Add(xmlReader.Name, obj = new JsonArray());

                                ((JsonArray)obj).Add(_parseToJsonObject(xmlReader));
                            }
                        }
                        catch(Exception err) {
                            throw new WebConversionException("Conversie failed in element '" + name + "'.", err);
                        }
                    }
                    break;
                }
            }
        }
        private                 JsonArray                   _parseToJsonArray(XmlTextReader xmlReader)
        {
            JsonArray   rtn = new JsonArray();

            if (!xmlReader.IsEmptyElement) {
                for (;;) {
                    _parseReadNode(xmlReader);

                    switch(xmlReader.NodeType) {
                    case XmlNodeType.EndElement:
                        return rtn;

                    case XmlNodeType.Element:
                        if (xmlReader.Name != "row")
                            throw new WebConversionException("Expect 'row' element.");

                        rtn.Add(_parseToJsonObject(xmlReader));
                        break;
                    }
                }
            }

            return rtn;
        }
        private     static      string                      _parseElementValue(XmlTextReader xmlReader)
        {
            if (!xmlReader.IsEmptyElement) {
                string  rtn = "";

                for (;;) {
                    _parseReadNode(xmlReader);

                    switch(xmlReader.NodeType) {
                    case XmlNodeType.EndElement:
                        return rtn;

                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        rtn += xmlReader.Value;
                        break;

                    default:
                        throw new WebConversionException("Value element has a child element ('" + xmlReader.NodeType.ToString() + "').");
                    }
                }
            }
            else {
                if (xmlReader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance") == "true")
                    return null;

                return "";
            }
        }
        private     static      void                        _parseReadNode(XmlTextReader xmlReader)
        {
            if (!xmlReader.Read())
                throw new WebConversionException("Reading EOF on xml-document.");
        }
        private     static      object                      _jsonConvertValue(char t, string svalue)
        {
            switch(t) {
            case 's':   return svalue;
            case 'i':   return !string.IsNullOrEmpty(svalue) ? (object)int.Parse(svalue, CultureInfo.InvariantCulture)   : null;
            case 'n':   return !string.IsNullOrEmpty(svalue) ? (object)float.Parse(svalue, CultureInfo.InvariantCulture) : null;

            case 'b':
                switch(svalue) {
                case null:      return null;
                case "":        return null;
                case "0":       return false;
                case "1":       return true;
                default:        throw new WebInvalidValueException("Invalid boolean value '" + svalue + "'.");
                }

            default:
                throw new WebConversionException("Invalid json-typeinfo in '" + t + "'.");
            }
        }
    }
}
