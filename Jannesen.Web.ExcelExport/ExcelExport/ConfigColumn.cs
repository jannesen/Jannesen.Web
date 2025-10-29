using System;
using System.Collections.Generic;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.ExcelExport.ExcelExport
{
    internal sealed class ConfigColumn
    {
        private readonly        int                         _col;
        private readonly        int                         _row;
        private                 int                         _colSpan;
        private readonly        int                         _rowSpan;
        private readonly        string                      _title;
        private readonly        string                      _fieldname;
        private readonly        string                      _format;
        private readonly        string                      _headerForegroundColor;
        private readonly        string                      _headerBackgroundColor;

        public                  int                         Col                         => _col;
        public                  int                         Row                         => _row;
        public                  int                         ColSpan                     => _colSpan;
        public                  int                         RowSpan                     => _rowSpan;
        public                  string                      Title                       => _title;
        public                  string                      Fieldname                   => _fieldname;
        public                  string                      Format                      => _format;
        public                  string                      HeaderForegroundColor       => _headerForegroundColor;
        public                  string                      HeaderBackgroundColor       => _headerBackgroundColor;

        public                                              ConfigColumn(int col, int row, WebCoreConfigReader configReader, ConfigColumn parent)
        {
            _col                    = col;
            _row                    = row;
            _colSpan                = 1;
            _rowSpan                = configReader.GetValueInt   ("row-span", 1, 1, 10);
            _title                  = configReader.GetValueString("title",     null);
            _fieldname              = configReader.GetValueString("fieldname", null);
            _headerForegroundColor  = configReader.GetValueString("header-foreground-color", (parent != null ? parent.HeaderForegroundColor : "00FFFFFF"));
            _headerBackgroundColor  = configReader.GetValueString("header-background-color", (parent != null ? parent.HeaderBackgroundColor : "003366FF"));

            if (_fieldname != null)
                _format    = configReader.GetValueString("format",    null);
        }

        internal                void                        SetColSpan(int colSpan)
        {
            _colSpan = colSpan;
        }
    }
}
