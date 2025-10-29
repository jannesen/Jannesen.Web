using System;
using System.IO;
using System.Threading;
using Microsoft.Data.SqlClient;
using Jannesen.Web.Core.Impl;
using Jannesen.Web.MSSql.Library;
using Jannesen.Web.ExcelExport.ExcelExport;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

namespace Jannesen.Web.ExcelExport
{
    [WebCoreHttpHandlerAttribute("sql-excelexport")]
    public class HttpHandlerExcelExport: HttpHandlerMSSql
    {
        private     readonly    ConfigSheetList             _sheets;
        private static readonly Lock                        _singleLock = new Lock();

        public                                              HttpHandlerExcelExport(WebCoreConfigReader configReader): base(configReader)
        {
            ArgumentNullException.ThrowIfNull(configReader);

            _sheets = new ConfigSheetList();

            if (configReader.hasChildren) {
                while (configReader.ReadNextElement()) {
                    switch(configReader.ElementName) {
                    case    "parameter":    ParseParameter(configReader);                       break;
                    case    "sheet":        _sheets.Add(new ConfigSheet(configReader));         break;
                    default:                configReader.InvalidElement();                      break;
                    }
                }
            }

            // Workaround a initalization problem
            using (SpreadsheetDocument.Create(new MemoryStream(), SpreadsheetDocumentType.Workbook)) {
            }
        }

        protected   override    WebCoreResponse             Process(WebCoreCall httpCall, SqlDataReader dataReader)
        {
            WebCoreResponseBuffer   webResponseBuffer = new WebCoreResponseBuffer("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false, false);

            HandleResponseOptions(webResponseBuffer, dataReader);

            lock(_singleLock) {
                using (MemoryStream buffer = new MemoryStream(4096000)) {
                    ExcelExport.ExportToExcel.Export(_sheets, dataReader, buffer);
                    webResponseBuffer.SetData(buffer);
                }
            }

            return webResponseBuffer;
        }
    }
}
