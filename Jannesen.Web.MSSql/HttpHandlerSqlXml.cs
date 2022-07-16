using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml;
using Jannesen.Web.Core.Impl;
using Jannesen.Web.MSSql.Library;

namespace Jannesen.Web.MSSql.Sqx
{
    [WebCoreAttribureHttpHandler("sql-xml")]
    public class HttpHandlerSqlXml: HttpHandlerMSSql
    {
        private readonly        bool                        _xmlIdent;

        public      override    string                      Mimetype
        {
            get {
                return "text/xml";
            }
        }

        public                                              HttpHandlerSqlXml(WebCoreConfigReader configReader): base(configReader)
        {
            _xmlIdent       = configReader.GetValueBool("xml-ident", false);

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
            WebCoreResponseBuffer   webResponseBuffer = new WebCoreResponseBuffer("text/xml; charset=utf-8", this.Public, true);

            if (HandleResponseOptions(webResponseBuffer, dataReader) == HttpStatusCode.OK) {
                try {
                    using (MemoryStream buffer = new MemoryStream(0x10000)) {
                        using (StreamWriter textStream  = new StreamWriter(buffer, new System.Text.UTF8Encoding(false), 1024, true)) {
                            if (_xmlIdent)
                                _fetchXmlIdent(textStream, dataReader);
                            else
                                _fetchData(textStream, dataReader);
                        }

                        webResponseBuffer.SetData(buffer);
                    }
                }
                catch(Exception err) {
                    throw new WebResponseException("Write of XML-BODY failed.", err);
                }
            }

            return webResponseBuffer;
        }

        private     static      void                        _fetchXmlIdent(StreamWriter textStream, SqlDataReader dataReader)
        {
            using (MemoryStream xmlMemory = new MemoryStream()) {
                using (StreamWriter xmlStream = new StreamWriter(xmlMemory, new System.Text.UTF8Encoding(false), 1024, true)) {
                    _fetchData(xmlStream, dataReader);
                }

                xmlMemory.Seek(0, SeekOrigin.Begin);

                using (var xmlInput = new XmlTextReader(new StreamReader(xmlMemory, new System.Text.UTF8Encoding(false))) { DtdProcessing=DtdProcessing.Prohibit, XmlResolver=null }) {
                    using (XmlTextWriter xmlWriter = new XmlTextWriter(textStream)) {
                        xmlWriter.Formatting = Formatting.Indented;
                        xmlWriter.Indentation = 1;
                        xmlWriter.IndentChar = '\t';
                        xmlWriter.WriteStartDocument();

                        while (!xmlInput.EOF)
                            xmlWriter.WriteNode(xmlInput, false);
                    }
                }
            }
        }
        private     static      void                        _fetchData(StreamWriter textStream, SqlDataReader dataReader)
        {
            bool        fempty = true;

            do {
                while(dataReader.Read()) {
                    textStream.Write(dataReader.GetString(0));
                    fempty = false;
                }
            }
            while (dataReader.NextResult());

            if (fempty)
                textStream.Write("<empty/>");
        }
    }
}
