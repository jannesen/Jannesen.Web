using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Jannesen.Web.MSSql.Library;
using Jannesen.Web.Core;

namespace Jannesen.Web.MSSql;

[WebCoreHttpHandler("sql-raw")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
public class HttpHandlerSqlRaw: HttpHandlerMSSql
{
    public                                                  HttpHandlerSqlRaw(WebCoreConfigReader configReader): base(configReader)
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

    protected override    async Task<IWebCoreResponse>      ProcessAsync(WebCoreCall httpCall, SqlDataReader dataReader, CancellationToken ct)
    {
        var webResponseBuffer = new WebCoreResponseBuffer(null, false);

        if (await HandleResponseOptionsAsync(webResponseBuffer, dataReader, ct) == HttpStatusCode.OK) {
            if (await dataReader.ReadAsync(ct)) {
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
