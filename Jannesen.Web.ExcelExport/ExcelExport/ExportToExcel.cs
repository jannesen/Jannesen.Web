using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Jannesen.Web.ExcelExport.ExcelExport
{
    internal sealed class ExportToExcel
    {
        private sealed class ProcessConfigSheet
        {
            public      ConfigSheet                 Config;
            public      ProcessConfigColumn[]       Columns;

            public                                  ProcessConfigSheet(ConfigSheet config)
            {
                this.Config  = config;
                this.Columns = new ProcessConfigColumn[config.Columns.Count];

                for (var c = 0 ; c < config.Columns.Count ; ++c) {
                    this.Columns[c] = new ProcessConfigColumn(config.Columns[c]);
                }
            }
        }

        private sealed class ProcessConfigColumn
        {
            public      ConfigColumn                Config;
            public      string                      ExcelColumnName;
            public      uint                        HeaderStyleIndex;
            public      uint                        DataStyleIndex;
            public      uint                        DataStyleIndexOdd;
            public      int                         FieldNr;
            public      Type                        SqlType;

            public                                  ProcessConfigColumn(ConfigColumn config)
            {
                this.Config = config;
            }
        }

        private                 SpreadsheetDocument         _document;

        public      static      void                        Export(ConfigSheetList configSheets, SqlDataReader dataReader, Stream outputStream)
        {
            var config = new ProcessConfigSheet[configSheets.Count];

            for (var s = 0 ; s < configSheets.Count ; ++s) {
                config[s] = new ProcessConfigSheet(configSheets[s]);
            }

            (new ExportToExcel())._process(config, dataReader, outputStream);
        }

        private                 void                        _process(ProcessConfigSheet[] config, SqlDataReader dataReader, Stream outputStream)
        {
            using (_document = SpreadsheetDocument.Create(outputStream, SpreadsheetDocumentType.Workbook)) {
                _document.AddWorkbookPart();
                _document.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                _createStyleSheet(config);
                _createSheets(config, dataReader);

                _document.WorkbookPart.Workbook.Save();
            }
        }
        private                 void                        _createStyleSheet(ProcessConfigSheet[] config)
        {
            var excelStyleSheet = new ExcelStyleSheet();

            foreach(var dataSetConfig in config) {
                foreach(var colcnf in dataSetConfig.Columns) {
                    var cfg = colcnf.Config;

                    if (cfg.Fieldname != null) {
                        colcnf.DataStyleIndex    = excelStyleSheet.GetCellFormatData(cfg, dataSetConfig.Config, false);
                        colcnf.DataStyleIndexOdd = excelStyleSheet.GetCellFormatData(cfg, dataSetConfig.Config, true);
                    }

                    if (cfg.Title != null)
                        colcnf.HeaderStyleIndex = excelStyleSheet.GetCellFormatHeader(cfg, dataSetConfig.Config);
                }
            }

            excelStyleSheet.Save(_document.WorkbookPart);
        }
        private                 void                        _createSheets(ProcessConfigSheet[] config, SqlDataReader dataReader)
        {
            var sheetNumber = (uint)0;
            var sheets      = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

            _document.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));
            _document.WorkbookPart.Workbook.AppendChild(sheets);

            do {
                var sheetConfig   = config[sheetNumber++];
                var worksheetPart = _document.WorkbookPart.AddNewPart<WorksheetPart>();
                var worksheet     = new Worksheet();
                var sheetData     = new SheetData();

                _mapDataSet(sheetConfig, dataReader);
                _excelFreePanes(worksheet, sheetConfig.Config.FreezeColumn, sheetConfig.Config.HeaderRows);
                _setupColumnsWidth(sheetConfig, worksheet);
                _copyHeader(sheetConfig, sheetData);
                _copyData(sheetConfig, sheetData, dataReader);

                worksheet.AppendChild(sheetData);
                _columnsMerge(sheetConfig, worksheet);

                worksheetPart.Worksheet = worksheet;
                worksheet.Save();

                sheets.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheet()
                                    {
                                        Id      = _document.WorkbookPart.GetIdOfPart(worksheetPart),
                                        SheetId = sheetNumber,
                                        Name    = sheetConfig.Config.Name
                                    });
            }
            while (dataReader.NextResult());
        }
        private     static      void                        _mapDataSet(ProcessConfigSheet config, SqlDataReader dataReader)
        {
            foreach(var colcnf in config.Columns) {
                colcnf.ExcelColumnName = _excelColumnName(colcnf.Config.Col);

                if (colcnf.Config.Fieldname != null) {
                    try {
                        colcnf.FieldNr = dataReader.GetOrdinal(colcnf.Config.Fieldname);
                    }
                    catch
                    {
                        throw new KeyNotFoundException("Field '" + colcnf.Config.Fieldname + "' not in dataset ''" + config.Config.Name + "'.");
                    }

                    colcnf.SqlType = dataReader.GetFieldType(colcnf.FieldNr);
                }
                else
                    colcnf.FieldNr = -1;
            }
        }
        private     static      void                        _setupColumnsWidth(ProcessConfigSheet config, Worksheet worksheet)
        {
            {
                var columnsWidth    = config.Config.ColumnsWidth;
                var columns         = new Columns();

                for (var colnr = 0 ; colnr < columnsWidth.Count ; ++colnr) {
                    var  width  = columnsWidth[colnr];

                    if (width > 0) {
                        var  column = new Column() {
                                          Min         = ((uint)colnr + 1),
                                          Width       = width,
                                          CustomWidth = true
                                      };

                        while ((colnr + 1) < columnsWidth.Count && columnsWidth[colnr + 1] == width) {
                            ++colnr;
                        }

                        column.Max = ((uint)colnr + 1);

                        columns.Append(column);
                    }
                }

                if (columns.ChildElements.Count > 0)
                    worksheet.Append(columns);
            }
        }
        private     static      void                        _copyHeader(ProcessConfigSheet config, SheetData sheetData)
        {
            for (var row = 0 ; row < config.Config.HeaderRows ; ++row) {
                var headerRow  = new Row { RowIndex = (uint)row + 1 };

                foreach(var colcfg in config.Columns) {
                    if (colcfg.Config.Row == row && colcfg.Config.Title != null) {
                        var cell = new Cell() {
                                       CellReference   = colcfg.ExcelColumnName + (row+1).ToString(CultureInfo.InvariantCulture),
                                       DataType        = CellValues.String,
                                       StyleIndex      = colcfg.HeaderStyleIndex
                                   };
                        cell.Append(new CellValue() {
                                        Text = colcfg.Config.Title
                                    });
                        headerRow.Append(cell);
                    }
                }

                sheetData.Append(headerRow);
            }
        }
        private     static      void                        _copyData(ProcessConfigSheet config, SheetData sheetData, SqlDataReader dataReader)
        {
            var rownr = (uint)config.Config.HeaderRows;
            var r     = 0;

            while (dataReader.Read()) {
                var row    = new Row { RowIndex = ++rownr };
                var srownr = rownr.ToString(CultureInfo.InvariantCulture);
                var odd    = (++r % 2) == 1;

                foreach (var colcfg in config.Columns) {
                    if (colcfg.FieldNr >= 0) {
                        var cell = new Cell() {
                                       CellReference = colcfg.ExcelColumnName + srownr
                                   };


                        if (!dataReader.IsDBNull(colcfg.FieldNr)) {
                            CellValues      dataType;
                            string          textValue;

                            if (colcfg.SqlType == typeof(System.String)) {
                                dataType  = CellValues.String;
                                textValue = dataReader.GetString(colcfg.FieldNr);
                            }
                            else
                            if (colcfg.SqlType == typeof(System.Byte)) {
                                dataType  = CellValues.Number;
                                textValue = dataReader.GetByte(colcfg.FieldNr).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            if (colcfg.SqlType == typeof(System.Int16)) {
                                dataType  = CellValues.Number;
                                textValue = dataReader.GetInt16(colcfg.FieldNr).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            if (colcfg.SqlType == typeof(System.Int32)) {
                                dataType  = CellValues.Number;
                                textValue = dataReader.GetInt32(colcfg.FieldNr).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            if (colcfg.SqlType == typeof(System.DateTime)) {
                                dataType  = CellValues.Number;
                                textValue = dataReader.GetDateTime(colcfg.FieldNr).ToOADate().ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            if (colcfg.SqlType == typeof(System.Decimal)) {
                                dataType  = CellValues.Number;
                                textValue = Convert.ToDouble(dataReader.GetDecimal(colcfg.FieldNr)).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            if (colcfg.SqlType == typeof(System.Double)) {
                                dataType  = CellValues.Number;
                                textValue = dataReader.GetDouble(colcfg.FieldNr).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                                throw new NotImplementedException("Column type '" + colcfg.SqlType.Name + "' not implented.");

                            cell.CellValue     = new CellValue() { Text = textValue };

                            if (colcfg.Config.Format == null)
                                cell.DataType = dataType;
                        }

                        if (odd) {
                            if (colcfg.DataStyleIndexOdd != 0)
                                cell.StyleIndex = colcfg.DataStyleIndexOdd;
                        }
                        else {
                            if (colcfg.DataStyleIndex != 0)
                                cell.StyleIndex = colcfg.DataStyleIndex;
                        }

                        row.Append(cell);
                    }
                }

                sheetData.Append(row);
            }
        }
        private     static      void                        _columnsMerge(ProcessConfigSheet config, Worksheet worksheet)
        {
            foreach(var colcfg in config.Columns) {
                if (colcfg.Config.ColSpan > 1 || colcfg.Config.RowSpan > 1) {
                    _excelMergeCells(worksheet, colcfg.Config.Col,                             colcfg.Config.Row,
                                                colcfg.Config.Col + colcfg.Config.ColSpan - 1, colcfg.Config.Row + colcfg.Config.RowSpan - 1);
                }
            }
        }

        private     static      void                        _excelMergeCells(Worksheet worksheet, int col1, int row1, int col2, int row2)
        {
            var r = _excelCellName(col1, row1) + ":" + _excelCellName(col2, row2);

            _getMergeCells(worksheet).Append(new MergeCell()
                                                    {
                                                        Reference = new StringValue(r)
                                                    });
        }
        private     static      void                        _excelFreePanes(Worksheet worksheet, int col, int row)
        {
            var sheetView = new SheetView() {
                                TabSelected = true,
                                WorkbookViewId = (UInt32Value)0U
                            };

            sheetView.Append(new Pane() {
                                 HorizontalSplit = (col>0) ? new DoubleValue((double)col) : null,
                                 VerticalSplit   = new DoubleValue((double)row),
                                 TopLeftCell     = _excelCellName(col, row),
                                 ActivePane      = PaneValues.BottomRight,
                                 State           = PaneStateValues.Frozen
                             });

            var sheetViews = new SheetViews();

            sheetViews.Append(sheetView);
            worksheet.Append(sheetViews);
        }
        private     static      string                      _excelCellName(int colnr, int rownr)
        {
            return _excelColumnName(colnr) + (rownr+1).ToString(CultureInfo.InvariantCulture);
        }
        private     static      string                      _excelColumnName(int colnr)
        {
            if (colnr < 26)
                return ((char)('A' + colnr)).ToString(CultureInfo.InvariantCulture);

            var builder = new StringBuilder(2);

            builder.Append((char)('A' + (colnr / 26) - 1));
            builder.Append((char)('A' + (colnr % 26)    ));

            return builder.ToString();
        }
        private     static      MergeCells                  _getMergeCells(Worksheet worksheet)
        {
            if (worksheet.Elements<MergeCells>().Any())
                return worksheet.Elements<MergeCells>().First();

            var mergeCells = new MergeCells();

            // Insert a MergeCells object into the specified position.
            if (worksheet.Elements<CustomSheetView>().Any()) {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<CustomSheetView>().First());
            }
            else if (worksheet.Elements<DataConsolidate>().Any())
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<DataConsolidate>().First());
            }
            else if (worksheet.Elements<SortState>().Any())
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<SortState>().First());
            }
            else if (worksheet.Elements<AutoFilter>().Any())
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<AutoFilter>().First());
            }
            else if (worksheet.Elements<Scenarios>().Any())
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<Scenarios>().First());
            }
            else if (worksheet.Elements<ProtectedRanges>().Any())
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<ProtectedRanges>().First());
            }
            else if (worksheet.Elements<SheetProtection>().Any())
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetProtection>().First());
            }
            else if (worksheet.Elements<SheetCalculationProperties>().Any())
            {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetCalculationProperties>().First());
            }
            else {
                worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetData>().First());
            }

            return mergeCells;
        }
    }
}
