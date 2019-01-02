using System;
using System.Collections.Generic;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.ExcelExport.ExcelExport
{
    class ConfigColumn
    {
        private         int                         _col;
        private         int                         _row;
        private         int                         _colSpan;
        private         int                         _rowSpan;
        private         string                      _title;
        private         string                      _fieldname;
        private         string                      _format;
        private         string                      _headerForegroundColor;
        private         string                      _headerBackgroundColor;

        public          int                         Col
        {
            get {
                return _col;
            }
        }
        public          int                         Row
        {
            get {
                return _row;
            }
        }
        public          int                         ColSpan
        {
            get {
                return _colSpan;
            }
        }
        public          int                         RowSpan
        {
            get {
                return _rowSpan;
            }
        }
        public          string                      Title
        {
            get {
                return _title;
            }
        }
        public          string                      Fieldname
        {
            get {
                return _fieldname;
            }
        }
        public          string                      Format
        {
            get {
                return _format;
            }
        }
        public          string                      HeaderForegroundColor
        {
            get {
                return _headerForegroundColor;
            }
        }
        public          string                      HeaderBackgroundColor
        {
            get {
                return _headerBackgroundColor;
            }
        }

        public                                      ConfigColumn(int col, int row, WebCoreConfigReader configReader, ConfigColumn parent)
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

        internal        void                        SetColSpan(int colSpan)
        {
            _colSpan = colSpan;
        }
    }

    class ConfigColumnList: List<ConfigColumn>
    {
    }
}
