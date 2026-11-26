using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using Microsoft.Data.SqlClient;
using Jannesen.FileFormat.Json;
using Jannesen.Web.MSSql.Library;
using Jannesen.Web.Core;
using System.Threading.Tasks;
using System.Threading;

namespace Jannesen.Web.MSSql;

[WebCoreHttpHandler("sql-json2")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
public class HttpHandlerSqlXmlJson2: HttpHandlerMSSql
{
    private abstract class ResponseRoot
    {
        public  static          ResponseRoot                ParseType(WebCoreConfigReader configReader, string type, bool root)
        {
            switch(type) {
            case "object":
                return new ResponseObject(configReader, false, root);
            case "object:mandatory":
                return new ResponseObject(configReader, true, root);

            default:
                if (type.StartsWith("array:", StringComparison.Ordinal))
                    return new ResponseArray(configReader, type.Substring(6));

                return new ResponseValue(type);
            }
        }
        public  abstract        void                        Convert(JsonWriter jsonWriter, XmlReader? xmlReader);
        public  virtual         void                        ConvertValue(JsonWriter jsonWriter, string value)
        {
            throw new WebConversionException("Expect simple value.");
        }
    }
    private sealed class ResponseValue: ResponseRoot
    {
        private readonly        ValueConvertor              _valueConvertor;

        public                                              ResponseValue(string nativeType)
        {
            _valueConvertor = ValueConvertor.GetType(nativeType);
        }

        public  override        void                        Convert(JsonWriter jsonWriter, XmlReader? xmlReader)
        {
            if (xmlReader != null) {
                if (xmlReader.Name != "value")
                    throw new WebConversionException("Expect value element, got '" + xmlReader.Name + "'.");

                var value = "";

                if (!xmlReader.IsEmptyElement) {
                    if (!xmlReader.Read())
                        throw new WebConversionException("EOF while reading XML.");

                    if (xmlReader.NodeType != XmlNodeType.Text)
                        throw new WebConversionException("expect element value.");

                    value = xmlReader.Value!;

                    if (!xmlReader.Read())
                        throw new WebConversionException("EOF while reading XML.");

                    if (xmlReader.NodeType != XmlNodeType.EndElement)
                        throw new WebConversionException("expect end-element.");
                }

                _valueConvertor.ConvertXmlValueToJson(value, jsonWriter);
            }
            else {
                jsonWriter.WriteNull();
            }
        }
        public  override        void                        ConvertValue(JsonWriter jsonWriter, string value)
        {
            _valueConvertor.ConvertXmlValueToJson(value, jsonWriter);
        }
    }
    private sealed class ResponseArray: ResponseRoot
    {
        public                  ResponseRoot                Response;

        public                                              ResponseArray(WebCoreConfigReader configReader, string type)
        {
            Response = ResponseRoot.ParseType(configReader, type, false);
        }
        public  override        void                        Convert(JsonWriter jsonWriter, XmlReader? xmlReader)
        {
            jsonWriter.WriteStartArray();

            if (xmlReader != null) {
                if (!xmlReader.IsEmptyElement) {
                    while (_readNextElement(xmlReader))
                        Response.Convert(jsonWriter, xmlReader);
                }
            }

            jsonWriter.WriteEndArray();
        }
    }
    private sealed class ResponseObject: ResponseRoot
    {
        public struct ResponseObjectField
        {
            public                  string                      Name;
            public                  ResponseRoot                Response;

            public                                              ResponseObjectField(WebCoreConfigReader configReader)
            {
                Name = configReader.GetValueString("name");
                Response = ResponseRoot.ParseType(configReader, configReader.GetValueString("type"), false);
            }
        }

        public                  bool                        Mandatory;
        public                  bool                        Root;
        public                  ResponseObjectField[]       Fields;

        public                                              ResponseObject(WebCoreConfigReader configReader, bool mandatory, bool root)
        {
            Mandatory = mandatory;
            Root     = root;

            if (!configReader.hasChildren)
                throw new WebConfigException("Missing fields.", configReader);

            var fields = new List<ResponseObjectField>();

            while (configReader.ReadNextElement()) {
                switch(configReader.ElementName) {
                case    "field":    fields.Add(new ResponseObjectField(configReader));  break;
                default:            configReader.InvalidElement();                      break;
                }
            }

            Fields = fields.ToArray();
        }
        public  override        void                        Convert(JsonWriter jsonWriter, XmlReader? xmlReader)
        {
            var curpos = 0;

            jsonWriter.WriteStartObject();

            if (xmlReader != null) {
                while (xmlReader.MoveToNextAttribute()) {
                    var name  = _normalizeName(xmlReader.Name);
                    jsonWriter.WriteName(name);
                    _findByName(ref curpos, name).Response.ConvertValue(jsonWriter, xmlReader.Value);
                }

                xmlReader.MoveToElement();

                if (!xmlReader.IsEmptyElement) {
                    while (_readNextElement(xmlReader)) {
                        var name  = _normalizeName(xmlReader.Name);
                        var field = _findByName(ref curpos, name);

                        jsonWriter.WriteName(name);
                        field.Response.Convert(jsonWriter, xmlReader);
                    }
                }
            }
            else {
                if (Mandatory) {
                    if (Root)
                        throw new NoDataException("No data found.");
                    else
                        throw new WebConversionException("Expect object-element.");
                }
            }

            jsonWriter.WriteEndObject();
        }

        private                 ResponseObjectField         _findByName(ref int curpos, string name)
        {
            var fieldCount = Fields.Length;

            for (var i = curpos ; i < fieldCount ; ++i) {
                if (Fields[i].Name == name) {
                    curpos = i+1 < fieldCount ? i+1 : 0;
                    return Fields[i];
                }
            }

            for (var i = 0 ; i < curpos ; ++i) {
                if (Fields[i].Name == name) {
                    curpos = i+1 < fieldCount ? i+1 : 0;
                    return Fields[i];
                }
            }

            throw new KeyNotFoundException("Can't find field '" + name + "'.");
        }
        private         static  string                      _normalizeName(string name)
        {
            var pos = 0;
            int p;

            while ((p = name.IndexOf('_', pos)) >= 0) {
                if (p + 5 < name.Length && name[p + 1] == 'x' &&  name[p + 6] == '_') {
                    var d1 = _ishexdigit(name[p + 2]);  if (d1 < 0) goto next;
                    var d2 = _ishexdigit(name[p + 3]);  if (d2 < 0) goto next;
                    var d3 = _ishexdigit(name[p + 4]);  if (d3 < 0) goto next;
                    var d4 = _ishexdigit(name[p + 5]);  if (d4 < 0) goto next;
                    var v = (d1 << 12) | (d2 << 8) | (d3 << 4) | (d4);
                    name = string.Concat(name.AsSpan(0, p), new string((char)v, 1), name.AsSpan(p + 7));
                }
next:               pos = p + 1;
            }

            return name;
        }
        private         static  int                         _ishexdigit(char c) {
            if (c >= '0' && c <= '9')   return c - '0';
            if (c >= 'A' && c <= 'F')   return c - 'A' + 10;
            return -1;
        }
    }
    private sealed class ResponseMsg
    {
        public                  string?      Name;
        public required         ResponseRoot Format;
    }

    public    override          string                      Mimetype
    {
        get {
            return _jsmodule ? "application/javascript" : "application/json";
        }
    }

    private   readonly          bool                        _jsmodule;
    private   readonly          ResponseMsg[]?              _responses;

    public                                                  HttpHandlerSqlXmlJson2(WebCoreConfigReader configReader): base(configReader)
    {
        ArgumentNullException.ThrowIfNull(configReader);

        if (configReader.hasChildren) {
            var responses = new List<ResponseMsg>();

            while (configReader.ReadNextElement()) {
                switch(configReader.ElementName) {
                case "parameter":
                    ParseParameter(configReader);
                    break;

                case "response":
                    responses.Add(new ResponseMsg() {
                                        Name   = configReader.GetValueString("responsemsg", null),
                                        Format = ResponseRoot.ParseType(configReader, configReader.GetValueString("type"), true)
                                    });
                    break;

                default:
                    configReader.InvalidElement();
                    break;
                }
            }

            if (responses.Count > 0) {
                _responses = responses.ToArray();
            }
        }

        if (Path.EndsWith(".js", StringComparison.Ordinal)) {
            if (_responses == null)
                throw new WebConfigException("Missing response", configReader);

            _jsmodule = true;
        }
    }

    protected override    async Task<IWebCoreResponse>      ProcessAsync(WebCoreCall httpCall, SqlCommand sqlCommand, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(httpCall);
        ArgumentNullException.ThrowIfNull(sqlCommand);

        var responseBuffer = new WebCoreResponseBuffer(Mimetype + "; charset=utf-8", true);

        if (_responses == null) {
            await sqlCommand.ExecuteNonQueryAsync(ct);
        }
        else {
            using (var dataReader = await sqlCommand.ExecuteReaderAsync(ct)) {
                if (await HandleResponseOptionsAsync(responseBuffer, dataReader, ct) == HttpStatusCode.OK) {
                    var responseformat = (ResponseRoot?)null;

                    if (!await dataReader.ReadAsync(ct))
                        throw new WebResponseException("No datareceived from database.");

                    if (dataReader.GetName(0) == "responsemsg") {
                        responseformat = _findResponseMsg(dataReader.GetString(0));
                        await dataReader.NextResultAsync(ct);

                        if (!await dataReader.ReadAsync(ct)) {
                            throw new WebResponseException("No datareceived from database.");
                        }
                    }
                    else
                        responseformat = _findResponseMsg(null);

                    try {
                        using (var streamBuffer = responseBuffer.GetStreamWriter()) {
                            if (_jsmodule) {
#pragma warning disable CA1849 // Onderliggende stream is een MemoryStream; async heeft geen meerwaarde
                                streamBuffer.Write("define([], function() { return ");
#pragma warning restore CA1849
                            }

                            using (var jsonWriter = new JsonWriter(streamBuffer, true)) {
                                if (!await dataReader.IsDBNullAsync(0, ct)) {
                                    using (var xmlReader = dataReader.GetXmlReader(0)) {
                                        _readNextElement(xmlReader);
                                        responseformat.Convert(jsonWriter, xmlReader);
                                    }
                                }
                                else {
                                    responseformat.Convert(jsonWriter, null);
                                }
                            }

                            if (_jsmodule) {
#pragma warning disable CA1849 // Onderliggende stream is een MemoryStream; async heeft geen meerwaarde
                                streamBuffer.Write(" });");
#pragma warning restore CA1849
                            }
                        }
                    }
                    catch(NoDataException) {
                        throw;
                    }
                    catch(TaskCanceledException) {
                        throw;
                    }
                    catch(Exception err) {
                        throw new WebResponseException("Convertion from XML to JSON failed.", err);
                    }
                }
            }
        }

        return responseBuffer;
    }

    private   static            bool                        _readNextElement(XmlReader xmlReader)
    {
        ArgumentNullException.ThrowIfNull(xmlReader);

        for (;;) {
            if (!xmlReader.Read())
                throw new WebConversionException("Unexpected EOF in xml.");

            switch(xmlReader.NodeType) {
            case XmlNodeType.Element:       return true;
            case XmlNodeType.EndElement:    return false;
            }
        }
    }
    private                     ResponseRoot                _findResponseMsg(string? responsemsg)
    {
        if (_responses != null) {
            foreach(var x in _responses) {
                if (x.Name == responsemsg)
                    return x.Format;
            }
        }

        throw new WebResponseException("Unknown responsemsg received from database.");
    }
}
