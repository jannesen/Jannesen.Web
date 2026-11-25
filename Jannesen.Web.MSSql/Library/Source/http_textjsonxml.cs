using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using Jannesen.FileFormat.Json;
using Jannesen.Web.Core;

namespace Jannesen.Web.MSSql.Library.Source;

[WebCoreDataSource("textjsonxml")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
internal sealed class http_textjsonxml: WebCoreDataSource
{
    public                                                              http_textjsonxml(string name_args): base(name_args)
    {
    }

    public      override        IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        return [ WebCoreProcessorTextJson.Definition ];
    }
    public      override        WebCoreDataValue                        GetValue(WebCoreCall httpCall)
    {
        if (httpCall.Request.Method == "GET" || httpCall.Request.Method == "HEAD")
            throw new WebHandlerConfigException("TEXTXML-BODY not available for HTTP/GET.");

        var jsondoc = httpCall.GetProcessorResult(WebCoreProcessorTextJson.Definition).Document;

        using (var stringWriter = new StringWriter()) {
            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() { CloseOutput=false, OmitXmlDeclaration=true })) {
                if (jsondoc is JsonObject docObject)
                    _jsonToXmlElement(xmlWriter, "json-object", docObject);
                else
                if (jsondoc is JsonArray docArray)
                    _jsonToXmlElement(xmlWriter, "json-array", docArray);
                else
                    throw new WebRequestException("Invalid JSON content");
            }

            return new WebCoreDataValue(stringWriter.ToString());
        }
    }

    private                     void                                    _jsonToXmlElement(XmlWriter xmlWriter, string elementName, JsonObject jsonObject)
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
    private                     void                                    _jsonToXmlElement(XmlWriter xmlWriter, string elementName, JsonArray jsonArray)
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
    private     static          void                                    _jsonToXmlAttribute(XmlWriter xmlWriter, string attributeName, object? value)
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
    public      override        string                                  ToString()
    {
        return "textjsonxml";
    }
}
