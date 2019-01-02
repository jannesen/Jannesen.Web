using System;
using System.Collections.Generic;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.ExcelExport.ExcelExport
{
    class ConfigSheet
    {
        private         string                      _name;
        private         double                      _fontSize;
        private         string                      _backgroundColor;
        private         string                      _backgroundColorOdd;
        private         int                         _headerRows;
        private         int                         _freezeColumn;
        private         List<double>                _columnsWidth;
        private         ConfigColumnList            _columns;

        public          string                      Name
        {
            get {
                return _name;
            }
        }
        public          double                      FontSize
        {
            get {
                return _fontSize;
            }
        }
        public          string                      BackgroundColor
        {
            get {
                return _backgroundColor;
            }
        }
        public          string                      BackgroundColorOdd
        {
            get {
                return _backgroundColorOdd;
            }
        }
        public          int                         HeaderRows
        {
            get {
                return _headerRows;
            }
        }
        public          int                         FreezeColumn
        {
            get {
                return _freezeColumn;
            }
        }
        public          List<double>                ColumnsWidth
        {
            get {
                return _columnsWidth;
            }
        }
        public          ConfigColumnList            Columns
        {
            get {
                return _columns;
            }
        }

        public                                      ConfigSheet(WebCoreConfigReader configReader)
        {
            _parse(configReader);
            _validate();
        }

        private         void                        _parse(WebCoreConfigReader configReader)
        {
            _name                = configReader.GetValueString("name");
            _fontSize            = configReader.GetValueDouble("font-size", 10, 6, 100);
            _backgroundColor     = configReader.GetValueString("background-color",     null);
            _backgroundColorOdd  = configReader.GetValueString("background-color-odd", _backgroundColor);
            _headerRows   = 1;
            _freezeColumn = 0;
            _columnsWidth = new List<double>();
            _columns      = new ConfigColumnList();

            _parseColumns(0, 0, configReader, null, this);
        }
        private         int                         _parseColumns(int col, int row, WebCoreConfigReader configReader, ConfigColumn parent, ConfigSheet sheet)
        {
            int     startCol = col;

            if (configReader.hasChildren) {
                while (configReader.ReadNextElement()) {
                    switch(configReader.ElementName) {
                    case "column":
                        {
                            ConfigColumn    column = new ConfigColumn(col, row, configReader, parent);
                            _columns.Add(column);

                            double?     width     = configReader.GetValueDoubleNull("width", 1, 200);
                            bool        freepane  = configReader.GetValueBool("free-pane", false);

                            if (column.Fieldname != null) {
                                configReader.NoChildElements();
                            }
                            else {
                                if (configReader.hasChildren) {
                                    int n = _parseColumns(col, row + column.RowSpan, configReader, column, sheet);
                                    column.SetColSpan(n);
                                }
                            }

                            if (width.HasValue && column.ColSpan == 1) {
                                while (_columnsWidth.Count <= col)
                                    _columnsWidth.Add(-1);

                                _columnsWidth[col] = (width.Value) * (sheet.FontSize / 10.0);
                            }

                            col += column.ColSpan;

                            if (freepane)
                                _freezeColumn = col;

                            if (_headerRows < row + column.RowSpan)
                                _headerRows = row + column.RowSpan;
                        }
                        break;

                    default:
                        configReader.InvalidElement();
                        break;
                    }
                }
            }

            return col - startCol;
        }
        private         void                        _validate()
        {
            for(int c = 0 ; c < _columnsWidth.Count ; ++c) {
                if (_columnsWidth[c] == -1)
                    throw new Exception("With not set for column #" + c.ToString() + ".");
            }
        }
    }

    class ConfigSheetList:  List<ConfigSheet>
    {
    }
}
