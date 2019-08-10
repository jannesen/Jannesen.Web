using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using Jannesen.Web.Core;
using Jannesen.Web.Core.Impl;
using Jannesen.Web.MSSql.Library;
using Jannesen.Web.ExcelExport.ExcelExport;

namespace Jannesen.Web.ExcelExport
{
    [WebCoreAttribureHttpHandler("sql-excelexport")]
    public class HttpHandlerExcelExport: HttpHandlerMSSql
    {
        private     readonly    ConfigSheetList             _sheets;
        private static readonly object                      _singleLock = new Object();

        public                                              HttpHandlerExcelExport(WebCoreConfigReader configReader): base(configReader)
        {
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
        }

        protected   override    WebCoreResponse             Process(WebCoreCall httpCall, SqlDataReader dataReader)
        {
            WebCoreResponseBuffer   webResponseBuffer = new WebCoreResponseBuffer("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", false, false);

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
