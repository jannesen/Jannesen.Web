using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Jannesen.Web.MSSql.Library;
using Jannesen.Web.Core;

namespace Jannesen.Web.MSSql;

[WebCoreHttpHandler("sql-xml")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
public class HttpHandlerSqlXml: HttpHandlerMSSql
{
    private   readonly          bool                         _xmlIdent;
    private   readonly          Encoding                      _charset;

    public    override          string                      Mimetype            => "text/xml";

    public                                                  HttpHandlerSqlXml(WebCoreConfigReader configReader): base(configReader)
    {
        ArgumentNullException.ThrowIfNull(configReader);

        _xmlIdent       = configReader.GetValueBool("xml-ident", false);

        var charset = configReader.GetValueString("charset", null);
        if (charset != null) {
            try {
                _charset =  Encoding.GetEncoding(charset);
            }
            catch(Exception) {
                throw new FormatException("Unknown charset '" + charset + "'.");
            }
        }
        else {
            _charset =  new UTF8Encoding(false);
        }

        if (configReader.hasChildren) {
            while (configReader.ReadNextElement()) {
                switch(configReader.ElementName) {
                case    "parameter":    ParseParameter(configReader);               break;
                default:                configReader.InvalidElement();              break;
                }
            }
        }
    }

    protected override    async Task<IWebCoreResponse>      ProcessAsync(WebCoreCall httpCall, SqlDataReader dataReader, CancellationToken ct)
    {
        var webResponseBuffer = new WebCoreResponseBuffer("text/xml; charset=" + _charset.BodyName, true);

        if (await HandleResponseOptionsAsync(webResponseBuffer, dataReader, ct) == HttpStatusCode.OK) {
            try {
                using (var textStream  = webResponseBuffer.GetStreamWriter()) {
                    if (_xmlIdent) {
                        await _fetchXmlIdentAsync(textStream, dataReader, ct);
                    }
                    else {
                        await _fetchDataAsync(textStream, dataReader, ct);
                    }
                }
            }
            catch(TaskCanceledException) {
                throw;
            }
            catch(Exception err) {
                throw new WebResponseException("Write of XML-BODY failed.", err);
            }
        }

        return webResponseBuffer;
    }

    private   static      async Task                        _fetchXmlIdentAsync(StreamWriter textStream, SqlDataReader dataReader, CancellationToken ct)
    {
        using (var xmlMemory = new MemoryStream()) {
            using (var xmlStream = new StreamWriter(xmlMemory, new System.Text.UTF8Encoding(false), 1024, true)) {
                await _fetchDataAsync(xmlStream, dataReader, ct);
            }

            xmlMemory.Seek(0, SeekOrigin.Begin);

            using (var xmlInput = new XmlTextReader(new StreamReader(xmlMemory, new System.Text.UTF8Encoding(false))) { DtdProcessing=DtdProcessing.Prohibit, XmlResolver=null }) {
                using (var xmlWriter = new XmlTextWriter(textStream)) {
#pragma warning disable CA1849 // Onderliggende stream is een MemoryStream; async heeft geen meerwaarde
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlWriter.Indentation = 1;
                    xmlWriter.IndentChar = '\t';
                    xmlWriter.WriteStartDocument();

                    while (!xmlInput.EOF) {
                        xmlWriter.WriteNode(xmlInput, false);
                    }
#pragma warning restore CA1849
                }
            }
        }
    }
    private   static      async Task                        _fetchDataAsync(StreamWriter textStream, SqlDataReader dataReader, CancellationToken ct)
    {
        var fempty = true;

        do {
            while(await dataReader.ReadAsync(ct)) {
#pragma warning disable CA1849 // Onderliggende stream is een MemoryStream; async heeft geen meerwaarde
                textStream.Write(dataReader.GetString(0));
#pragma warning restore CA1849
                fempty = false;
            }
        }
        while (await dataReader.NextResultAsync(ct));

        if (fempty) {
#pragma warning disable CA1849 // Onderliggende stream is een MemoryStream; async heeft geen meerwaarde
            textStream.Write("<empty/>");
#pragma warning restore CA1849
        }
    }
}
