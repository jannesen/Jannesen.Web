using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Xml;
using Jannesen.FileFormat.Json;
using Jannesen.Web.Core.Impl;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.MSSql.Library.Source
{
    [WebCoreDataSourceAttribute("textjsonxml")]
    internal sealed class http_textjsonxml: WebCoreDataSource
    {
        public                                              http_textjsonxml(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            if (httpCall.Request.Method == "GET" || httpCall.Request.Method == "HEAD")
                throw new WebHandlerConfigException("TEXTXML-BODY not available for HTTP/GET.");

            var jsondoc = httpCall.RequestTextJson.Document;

            using (var stringWriter = new StringWriter()) {
#pragma warning disable CA2000 // refecence to stringWriter which is disposed
                var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() { CloseOutput=false, OmitXmlDeclaration=true });
#pragma warning restore CA2000

                if (jsondoc is JsonObject docObject)
                    _jsonToXmlElement(xmlWriter, "json-object", docObject);
                else
                if (jsondoc is JsonArray docArray)
                    _jsonToXmlElement(xmlWriter, "json-array", docArray);
                else
                    throw new WebRequestException("Invalid JSON content");

                xmlWriter.Flush();

                return new WebCoreDataValue(stringWriter.ToString());
            }
        }

        private                     void                    _jsonToXmlElement(XmlWriter xmlWriter, string elementName, JsonObject jsonObject)
        {
            xmlWriter.WriteStartElement(elementName);

            foreach(var item in jsonObject) {
                if (!(item.Value is JsonObject || item.Value is JsonArray))
                    _jsonToXmlAttribute(xmlWriter, item.Key, item.Value);
            }

            foreach(var item in jsonObject) {
                if (item.Value is JsonObject itemObject)
                    _jsonToXmlElement(xmlWriter, item.Key, itemObject);
            }

            foreach(var item in jsonObject) {
                if (item.Value is JsonArray itemArray)
                    _jsonToXmlElement(xmlWriter, item.Key, itemArray);
            }

            xmlWriter.WriteEndElement();
        }
        private                     void                    _jsonToXmlElement(XmlWriter xmlWriter, string elementName, JsonArray jsonArray)
        {
            xmlWriter.WriteStartElement(elementName);

            foreach(var item in jsonArray) {
                if (item is JsonObject itemObject)
                    _jsonToXmlElement(xmlWriter, "row", itemObject);
                else
                if (item is JsonArray itemArray)
                    _jsonToXmlElement(xmlWriter, "row", itemArray);
                else {
                    xmlWriter.WriteStartElement("row");
                    _jsonToXmlAttribute(xmlWriter, "value", item);
                }
            }

            xmlWriter.WriteEndElement();
        }
        private     static          void                    _jsonToXmlAttribute(XmlWriter xmlWriter, string attributeName, object? value)
        {
            if (value != null) {
                if (value is string vstring)
                    xmlWriter.WriteAttributeString("_s_" + attributeName, vstring);
                else
                if (value is Int64 vint64)
                    xmlWriter.WriteAttributeString("_i_" + attributeName, XmlConvert.ToString(vint64));
                else
                if (value is double vdouble)
                    xmlWriter.WriteAttributeString("_n_" + attributeName, XmlConvert.ToString(vdouble));
                else
                if (value is bool vbool)
                    xmlWriter.WriteAttributeString("_b_" + attributeName, vbool ? "1" : "0");
                else
                    throw new WebConversionException("Can't convert json '" + value.GetType().Name + "' to xml-attribute.");
            }
        }
        public      override        string                  ToString()
        {
            return "textjsonxml";
        }
    }
}
