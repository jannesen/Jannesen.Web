using System;
using System.Collections.Generic;
using System.Globalization;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.ExcelExport.ExcelExport
{
    internal sealed class ConfigSheet
    {
        private         string                      _name;
        private         double                      _fontSize;
        private         string                      _backgroundColor;
        private         string                      _backgroundColorOdd;
        private         int                         _headerRows;
        private         int                         _freezeColumn;
        private         List<double>                _columnsWidth;
        private         List<ConfigColumn>          _columns;

        public          string                      Name                    => _name;
        public          double                      FontSize                => _fontSize;
        public          string                      BackgroundColor         => _backgroundColor;
        public          string                      BackgroundColorOdd      => _backgroundColorOdd;
        public          int                         HeaderRows              => _headerRows;
        public          int                         FreezeColumn            => _freezeColumn;
        public          IReadOnlyList<double>       ColumnsWidth            => _columnsWidth;
        public          IReadOnlyList<ConfigColumn> Columns                 => _columns;

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
            _columns      = new List<ConfigColumn>();

            _parseColumns(0, 0, configReader, null, this);
        }
        private         int                         _parseColumns(int col, int row, WebCoreConfigReader configReader, ConfigColumn parent, ConfigSheet sheet)
        {
            var startCol = col;

            if (configReader.hasChildren) {
                while (configReader.ReadNextElement()) {
                    switch(configReader.ElementName) {
                    case "column": {
                            var column = new ConfigColumn(col, row, configReader, parent);
                            _columns.Add(column);

                            var width     = configReader.GetValueDoubleNull("width", 1, 200);
                            var freepane  = configReader.GetValueBool("free-pane", false);

                            if (column.Fieldname != null) {
                                configReader.NoChildElements();
                            }
                            else {
                                if (configReader.hasChildren) {
                                    var n = _parseColumns(col, row + column.RowSpan, configReader, column, sheet);
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
            for(var c = 0 ; c < _columnsWidth.Count ; ++c) {
                if (_columnsWidth[c] == -1)
                    throw new Exception("With not set for column #" + c.ToString(CultureInfo.InvariantCulture) + ".");
            }
        }
    }
}
