using System;
using System.Collections.Generic;
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
        private        readonly List<ConfigSheet>           _sheets;
        private static readonly Lock                        _singleLock = new Lock();

        public                                              HttpHandlerExcelExport(WebCoreConfigReader configReader): base(configReader)
        {
            ArgumentNullException.ThrowIfNull(configReader);

            _sheets = new List<ConfigSheet>();

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

        protected   override    IWebCoreResponse            Process(WebCoreCall httpCall, SqlDataReader dataReader)
        {
            var webResponseBuffer = new WebCoreResponseBuffer("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false);

            HandleResponseOptions(webResponseBuffer, dataReader);

            lock(_singleLock) {
                using (var stream = webResponseBuffer.GetStream()) {
                    ExportToExcel.Export(_sheets, dataReader, stream);
                }
            }

            return webResponseBuffer;
        }
    }
}
