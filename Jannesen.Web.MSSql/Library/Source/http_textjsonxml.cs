using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Xml;
using Jannesen.FileFormat.Json;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.MSSql.Library.Source
{
    [WebCoreAttributeDataSource("textjsonxml")]
    class http_textjsonxml: WebCoreDataSource
    {
        public                                              http_textjsonxml(string name_args): base(name_args)
        {
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            if (httpCall.Request.HttpMethod == "GET" || httpCall.Request.HttpMethod == "HEAD")
                throw new WebHandlerConfigException("TEXTXML-BODY not available for HTTP/GET.");

            object      jsondoc = httpCall.RequestTextJson.Document;

            using (StringWriter stringWriter = new StringWriter())
            {
                XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() { CloseOutput=false, OmitXmlDeclaration=true });

                if (jsondoc is JsonObject)
                    _jsonToXmlElement(xmlWriter, "json-object", (JsonObject)jsondoc);
                else
                if (jsondoc is JsonArray)
                    _jsonToXmlElement(xmlWriter, "json-array", (JsonArray)jsondoc);
                else
                    throw new WebRequestException("Invalid JSON content");

                xmlWriter.Flush();

                return new WebCoreDataValue(stringWriter.ToString());
            }
        }

        private                     void                    _jsonToXmlElement(XmlWriter xmlWriter, string elementName, JsonObject jsonObject)
        {
            xmlWriter.WriteStartElement(elementName);

            foreach(KeyValuePair<string, object> item in jsonObject) {
                if (!(item.Value is JsonObject || item.Value is JsonArray))
                    _jsonToXmlAttribute(xmlWriter, item.Key, item.Value);
            }

            foreach(KeyValuePair<string, object> item in jsonObject) {
                if (item.Value is JsonObject)
                    _jsonToXmlElement(xmlWriter, item.Key, (JsonObject)item.Value);
            }

            foreach(KeyValuePair<string, object> item in jsonObject) {
                if (item.Value is JsonArray)
                    _jsonToXmlElement(xmlWriter, item.Key, (JsonArray)item.Value);
            }

            xmlWriter.WriteEndElement();
        }
        private                     void                    _jsonToXmlElement(XmlWriter xmlWriter, string elementName, JsonArray jsonArray)
        {
            xmlWriter.WriteStartElement(elementName);

            foreach(object item in jsonArray) {
                if (item is JsonObject)
                    _jsonToXmlElement(xmlWriter, "row", (JsonObject)item);
                else
                if (item is JsonArray)
                    _jsonToXmlElement(xmlWriter, "row", (JsonArray)item);
                else {
                    xmlWriter.WriteStartElement("row");
                    _jsonToXmlAttribute(xmlWriter, "value", item);
                }
            }

            xmlWriter.WriteEndElement();
        }
        private                     void                    _jsonToXmlAttribute(XmlWriter xmlWriter, string attributeName, object value)
        {
            if (value != null) {
                if (value is string)
                    xmlWriter.WriteAttributeString("_s_" + attributeName, (string)value);
                else
                if (value is Int64)
                    xmlWriter.WriteAttributeString("_i_" + attributeName, XmlConvert.ToString((Int64)value));
                else
                if (value is double)
                    xmlWriter.WriteAttributeString("_n_" + attributeName, XmlConvert.ToString((double)value));
                else
                if (value is bool)
                    xmlWriter.WriteAttributeString("_b_" + attributeName, (bool)value ? "1" : "0");
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
