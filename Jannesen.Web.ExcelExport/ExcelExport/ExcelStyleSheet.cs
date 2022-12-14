using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Jannesen.Web.ExcelExport.ExcelExport
{
    internal sealed class ExcelStyleSheet
    {
        private readonly        NumberingFormats            _numberingFormats;
        private readonly        Fonts                       _fonts;
        private readonly        Fills                       _fills;
        private readonly        Borders                     _borders;
        private readonly        CellStyleFormats            _cellStyleFormats;
        private readonly        CellFormats                 _cellFormats;
        private readonly        CellStyles                  _cellStyles;
        private readonly        DifferentialFormats         _differentialFormats;
        private readonly        TableStyles                 _tableStyles;
        private                 uint                        _cellFormatId;
        private                 uint                        _numberingFormatsIndex;

        public                                              ExcelStyleSheet()
        {
            _numberingFormats    = new NumberingFormats();
            _fonts               = new Fonts();
            _fills               = new Fills();
            _borders             = new Borders();
            _cellStyleFormats    = new CellStyleFormats();
            _cellFormats         = new CellFormats();
            _cellStyles          = new CellStyles();
            _differentialFormats = new DifferentialFormats();
            _tableStyles         = new TableStyles();

            _defaults();
        }

        public                  uint                        GetCellFormatHeader(ConfigColumn configColumn, ConfigSheet configSheet)
        {
            Font            font = new Font()
                                    {
                                        FontName = new FontName() { Val = "Calibri"            },
                                        FontSize = new FontSize() { Val = configSheet.FontSize },
                                        Color    = new Color()    { Rgb = configColumn.HeaderForegroundColor }
                                    };

            PatternFill     patternFill = new PatternFill() { PatternType = PatternValues.Solid };
            patternFill.Append(new ForegroundColor(){ Rgb     = configColumn.HeaderBackgroundColor });
            patternFill.Append(new BackgroundColor(){ Indexed = 64 });
            Fill            fill = new Fill();
            fill.Append(patternFill);

            return _getCellFormat(new CellFormat()      // CellFormat #2
                                {
                                    Alignment      = new Alignment()
                                                        {
                                                            Horizontal = (configColumn.ColSpan > 1 ? HorizontalAlignmentValues.Center : HorizontalAlignmentValues.Left),
                                                            Vertical = VerticalAlignmentValues.Top
                                                        },
                                    NumberFormatId = 0,
                                    FontId         = _getFont(font),
                                    FillId         = _getFill(fill),
                                    BorderId       = 0,
                                    FormatId       = 0,
                                    ApplyAlignment = true,
                                    ApplyFont      = true,
                                    ApplyFill      = true
                                });
        }
        public                  uint                        GetCellFormatData(ConfigColumn configColumn, ConfigSheet configSheet, bool odd)
        {
            Font            font = new Font()
                                    {
                                        FontName = new FontName() { Val = "Calibri"            },
                                        FontSize = new FontSize() { Val = configSheet.FontSize },
                                    };
            Fill            fill = null;

            if (configSheet.BackgroundColor != null) {
                PatternFill     patternFill = new PatternFill() { PatternType = PatternValues.Solid };
                patternFill.Append(new ForegroundColor(){ Rgb     = (odd) ? configSheet.BackgroundColorOdd : configSheet.BackgroundColor });
                patternFill.Append(new BackgroundColor(){ Indexed = 64 });
                fill = new Fill();
                fill.Append(patternFill);
            }
            return _getCellFormat(new CellFormat()
                                {
                                    NumberFormatId    = configColumn.Format != null ? _getNumberingFormat(configColumn.Format) : 0,
                                    FontId            = _getFont(font),
                                    FillId            = (fill != null) ? _getFill(fill) : 0,
                                    ApplyNumberFormat = true,
                                    ApplyFont         = true,
                                    ApplyFill         = true
                                });
        }

        public                  void                        Save(WorkbookPart workbookPart)
        {
            _numberingFormats.Count     = (uint)_numberingFormats.ChildElements.Count;
            _fonts.Count                = (uint)_fonts.ChildElements.Count;
            _fills.Count                = (uint)_fills.ChildElements.Count;
            _borders.Count              = (uint)_borders.ChildElements.Count;
            _cellStyleFormats.Count     = (uint)_cellStyleFormats.ChildElements.Count;
            _cellFormats.Count          = (uint)_cellFormats.ChildElements.Count;
            _cellStyles.Count           = (uint)_cellStyles.ChildElements.Count;
            _differentialFormats.Count  = (uint)_differentialFormats.ChildElements.Count;
            _tableStyles.Count          = (uint)_tableStyles.ChildElements.Count;

            Stylesheet  stylesheet = new Stylesheet();
            stylesheet.Append(_numberingFormats,
                              _fonts,
                              _fills,
                              _borders,
                              _cellStyleFormats,
                              _cellFormats,
                              _cellStyles,
                              _differentialFormats,
                              _tableStyles);

            workbookPart.AddNewPart<WorkbookStylesPart>().Stylesheet = stylesheet;

            stylesheet.Save();
        }

        private                 void                        _defaults()
        {
            _cellFormatId = 0;
            _numberingFormatsIndex = 165;

            // Default font (data)
            _fonts.Append(new Font()
                            {
                                FontName = new FontName() { Val = "Calibri" },
                                FontSize = new FontSize() { Val = 10        }
                            });

            _fills.Append(new Fill()
                            {
                                PatternFill = new PatternFill()
                                    {
                                        PatternType = PatternValues.None
                                    }
                            });
            _fills.Append(new Fill()
                            {
                                PatternFill = new PatternFill()
                                    {
                                        PatternType = PatternValues.Gray125
                                    }
                            });

            _borders.Append(new Border()
                                {
                                    LeftBorder      = new LeftBorder(),
                                    RightBorder     = new RightBorder(),
                                    TopBorder       = new TopBorder(),
                                    BottomBorder    = new BottomBorder(),
                                    DiagonalBorder  = new DiagonalBorder()
                                });

            _cellStyleFormats.Append(new CellFormat()
                                        {
                                            NumberFormatId = 0,
                                            FontId         = 0,
                                            FillId         = 0,
                                            BorderId       = 0,
                                            FormatId       = 0
                                        });

            _cellFormats.Append(new CellFormat()
                                    {
                                        NumberFormatId = 0,
                                        FontId         = 0,
                                        FillId         = 0,
                                        BorderId       = 0,
                                        FormatId       = _cellFormatId++
                                    });

            _cellStyles.Append(new CellStyle()
                                {
                                    Name      = "Normal",
                                    FormatId  = 0,
                                    BuiltinId = 0
                                });

            _tableStyles.DefaultTableStyle = "TableStyleMedium2";
            _tableStyles.DefaultPivotStyle = "PivotStyleLight16";
        }

        private                 uint                        _getFont(Font font)
        {
            return _getOpenXmlElement(_fonts, font);
        }
        private                 uint                        _getFill(Fill fill)
        {
            return _getOpenXmlElement(_fills, fill);
        }
        private                 uint                        _getCellFormat(CellFormat cellFormat)
        {
            return _getOpenXmlElement(_cellFormats, cellFormat);
        }
        private static          uint                        _getOpenXmlElement(OpenXmlElement list, OpenXmlElement element)
        {
            int i;

            for (i = 0 ; i < list.ChildElements.Count ; ++i) {
                if (list.ChildElements[i].OuterXml == element.OuterXml)
                    return (uint)i;
            }

            list.Append(element);

            return (uint)i;
        }
        private                 uint                        _getNumberingFormat(string format)
        {
            switch(format) {
            case "0":                           return  1;
            case "0.00":                        return  2;
            case "#,##0":                       return  3;
            case "#,##0.00":                    return  4;
            case "0%":                          return  9;
            case "0.00%":                       return 10;
            case "0.00E+00":                    return 11;
            case "# ?/?":                       return 12;
            case "# ??/??":                     return 13;
            case "d/m/yyyy":                    return 14;
            case "d-mmm-yy":                    return 15;
            case "d-mmm":                       return 16;
            case "mmm-yy":                      return 17;
            case "h:mm tt":                     return 18;
            case "h:mm:ss tt":                  return 19;
            case "H:mm":                        return 20;
            case "H:mm:ss":                     return 21;
            case "m/d/yyyy H:mm":               return 22;
            case "#,##0 ;(#,##0)":              return 37;
            case "#,##0 ;[Red](#,##0)":         return 38;
            case "#,##0.00;(#,##0.00)":         return 39;
            case "#,##0.00;[Red](#,##0.00)":    return 40;
            case "mm:ss":                       return 45;
            case "[h]:mm:ss":                   return 46;
            case "mmss.0":                      return 47;
            case "##0.0E+0":                    return 48;
            case "@":                           return 49;

            default:
                {
                    foreach(NumberingFormat f in _numberingFormats) {
                        if (f.FormatCode == format)
                            return f.NumberFormatId;
                    }

                    {
                        _numberingFormats.Append(new NumberingFormat()
                                                        {
                                                            NumberFormatId = _numberingFormatsIndex,
                                                            FormatCode     = format
                                                        });
                        return _numberingFormatsIndex++;
                    }
                }
            }
        }
    }
}
