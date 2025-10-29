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

        protected   override    WebCoreResponse             Process(WebCoreCall httpCall, SqlDataReader dataReader)
        {
            WebCoreResponseBuffer   webResponseBuffer = new WebCoreResponseBuffer(null, this.Public, false);

            if (HandleResponseOptions(webResponseBuffer, dataReader) == HttpStatusCode.OK) {
                if (dataReader.Read()) {
                    for (int col= 0 ; col < dataReader.FieldCount ; ++col) {
                        switch(dataReader.GetName(col).ToLowerInvariant()) {
                        case "content-type": {
                                SqlString   String = dataReader.GetSqlString(col);

                                if (!String.IsNull)
                                    webResponseBuffer.ContentType = String.Value;
                            }
                            break;

                        case "text": {
                                SqlString text = dataReader.GetSqlString(col);

                                if (!text.IsNull) {
                                    webResponseBuffer.SetData(System.Text.Encoding.UTF8.GetBytes(text.Value));
                                }
                            }
                            break;

                        case "data": {
                                SqlBinary data = dataReader.GetSqlBinary(col);

                                if (!data.IsNull)
                                    webResponseBuffer.SetData(data.Value, data.Length);
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
