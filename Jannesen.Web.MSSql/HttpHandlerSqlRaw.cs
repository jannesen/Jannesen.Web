using System;
using System.Net;
using System.Data.SqlTypes;
using Microsoft.Data.SqlClient;
using Jannesen.Web.Core.Impl;
using Jannesen.Web.MSSql.Library;

namespace Jannesen.Web.MSSql
{
    [WebCoreHttpHandlerAttribute("sql-raw")]
    public class HttpHandlerSqlRaw: HttpHandlerMSSql
    {
        public                                              HttpHandlerSqlRaw(WebCoreConfigReader configReader): base(configReader)
        {
            ArgumentNullException.ThrowIfNull(configReader);

            if (configReader.hasChildren) {
                while (configReader.ReadNextElement()) {
                    switch(configReader.ElementName) {
                    case    "parameter":    ParseParameter(configReader);               break;
                    default:                configReader.InvalidElement();              break;
                    }
                }
            }
        }

        protected   override    IWebCoreResponse            Process(WebCoreCall httpCall, SqlDataReader dataReader)
        {
            var webResponseBuffer = new WebCoreResponseBuffer(null, false);

            if (HandleResponseOptions(webResponseBuffer, dataReader) == HttpStatusCode.OK) {
                if (dataReader.Read()) {
                    for (var col= 0 ; col < dataReader.FieldCount ; ++col) {
                        switch(dataReader.GetName(col).ToLowerInvariant()) {
                        case "content-type": {
                                var String = dataReader.GetSqlString(col);

                                if (!String.IsNull) {
                                    webResponseBuffer.ContentType = String.Value;
                                }
                            }
                            break;

                        case "text": {
                                var text = dataReader.GetSqlString(col);

                                if (!text.IsNull) {
                                    webResponseBuffer.Data = System.Text.Encoding.UTF8.GetBytes(text.Value);
                                }
                            }
                            break;

                        case "data": {
                                var data = dataReader.GetSqlBinary(col);

                                if (!data.IsNull) {
                                    webResponseBuffer.Data = new ReadOnlyMemory<byte>(data.Value, 0, data.Length);
                                }
                            }
                            break;
                        }
                    }
                }
            }

            return webResponseBuffer;
        }
    }
}
