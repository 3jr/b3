﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Windows;

namespace BallOnTiltablePlate.TimoSchmetzer.Utilities
{
    class ExcelUtilities
    {
        public class ExcelDiagramCreator
        {
            /// <summary>
            /// Used to save Data.
            /// </summary>
            private Dictionary<string, List<System.Windows.Point>> Dataseries = new Dictionary<string,List<System.Windows.Point>>();

            /// <summary>
            /// X Axis name Displayed in the Diagram
            /// </summary>
            public string AxisNameX { get; set; }

            /// <summary>
            /// Y Axis name Displayed in the Diagram
            /// </summary>
            public string AxisNameY { get; set; }

            /// <summary>
            /// Diagram Title Displayed in the Diagram and Name of the Diagram
            /// </summary>
            public string DiagramTitle { get; set; }

            public void AddPoint(string series, System.Windows.Point p)
            {
                if (Dataseries.ContainsKey(series))
                {
                    Dataseries[series].Add(p);
                }
                else
                {
                    List<System.Windows.Point> l = new List<System.Windows.Point>();
                    l.Add(p);
                    Dataseries.Add(series, l);
                }
            }

            public void AddPoints(string series, IEnumerable<System.Windows.Point> p)
            {
                if (Dataseries.ContainsKey(series))
                {
                    Dataseries[series].AddRange(p);
                }
                else
                {
                    Dataseries.Add(series, new List<System.Windows.Point>(p));
                }
            }

            public void GenerateAndShowDiagram()
            {
                List<Tuple<string, IEnumerable<System.Windows.Point>>> Data = new List<Tuple<string, IEnumerable<System.Windows.Point>>>();
                foreach (KeyValuePair<string, List<System.Windows.Point>> kvp in Dataseries)
                {
                    Data.Add(new Tuple<string, IEnumerable<System.Windows.Point>>(kvp.Key, kvp.Value));
                }
                try
                {
                    GenerateAndShowDiagram(DiagramTitle, AxisNameX, AxisNameY, Data);
                }
                catch(Exception e)
                {
                    MessageBox.Show("Unable to Create Diagram. ExceptionMessage:" + e.Message);
                }
            }

            /// <summary>
            /// Generates and shows a diagram with the specified data.
            /// </summary>
            /// <param name="Data">Ienumerable<Datarow<Dataheading,IEnumerable<Datapoints>>></param>
            private static void GenerateAndShowDiagram(string ChartTitle, string CategryAxisName, string ValueAxisName, IEnumerable<Tuple<string, IEnumerable<System.Windows.Point>>> Data)
            {
                #region InitExel
                Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();

                if (xlApp == null)
                {
                    Console.WriteLine("EXCEL could not be started. Check that your office installation and project references are correct.");
                    return;
                }
                xlApp.Visible = true;

                Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                Worksheet ws = (Worksheet)wb.Worksheets[1];

                if (ws == null)
                {
                    Console.WriteLine("Worksheet could not be created. Check that your office installation and project references are correct.");
                }
                #endregion
                #region WriteValuesAndGenerateDiagram
                Chart c = wb.Charts.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                c.ChartType = XlChartType.xlXYScatter;
                c.SetSourceData(ws.get_Range("A1", "B1"), XlRowCol.xlRows);
                int CollectionNumber = 0;
                Cellwriter writer = new Cellwriter(ws);
                writer.WriteColumn = 1;
                foreach (Tuple<string, IEnumerable<System.Windows.Point>> Datarow in Data)
                {
                    //Generate headings
                    writer.WriteRow = 1;
                    writer.writeCell("X");
                    writer.WriteColumn++;
                    writer.writeCell(Datarow.Item1);
                    writer.WriteColumn--;
                    //Write Points
                    foreach (System.Windows.Point datapoint in Datarow.Item2)
                    {
                        writer.WriteRow++;
                        writer.writeCell(datapoint.X);
                        writer.WriteColumn++;
                        writer.writeCell(datapoint.Y);
                        writer.WriteColumn--;
                    }
                    writer.WriteColumn += 2;
                    //Generate Dataseries
                    CollectionNumber++;
                    if (CollectionNumber > 1)
                    {
                        c.SeriesCollection().NewSeries();
                    }
                    Series s = c.SeriesCollection(CollectionNumber);
                    int DataLength = Datarow.Item2.ToArray().Length;
                    
                    s.XValues = xlApp.Range[ws.Cells[2, writer.WriteColumn - 2], ws.Cells[DataLength + 1, writer.WriteColumn - 2]];
                    s.Values = xlApp.Range[ws.Cells[2, writer.WriteColumn - 1], ws.Cells[DataLength + 1, writer.WriteColumn - 1]];
                    s.Name = Datarow.Item1;
                }

                c.HasTitle = true;
                c.ChartTitle.Text = ChartTitle;
                ((Axis)c.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary)).HasTitle = true;
                ((Axis)c.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary)).AxisTitle.Text = CategryAxisName;
                ((Axis)c.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary)).HasTitle = true;
                ((Axis)c.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary)).AxisTitle.Text = ValueAxisName;

                c.Location(XlChartLocation.xlLocationAsNewSheet, ChartTitle);
                #endregion
            }
        }
        public class Cellwriter
        {
            public Cellwriter() { }
            public Cellwriter(Worksheet WriteSheet)
            {
                this.WriteSheet = WriteSheet;
            }

            /// <summary>
            /// The Point to Write at.
            /// x:Row 1->1
            /// y:Column 1->A
            /// </summary>
            public int WriteColumn;

            /// <summary>
            /// The Point to Write at.
            /// x:Row 1->1
            /// y:Column 1->A
            /// </summary>
            public int WriteRow;

            /// <summary>
            /// The ExcelSheet to write to.
            /// </summary>
            public Worksheet WriteSheet { get; set; }

            /// <summary>
            /// Method writes the specified value at the WritePosition in WriteSheet.
            /// </summary>
            /// <param name="value">Value to be written.</param>
            public void writeCell(dynamic value)
            {
                writeCell(WriteSheet, WriteRow, WriteColumn, value);
            }

            /// <summary>
            /// Writes a cell
            /// </summary>
            /// <param name="Worksheet">The worksheet to write to</param>
            /// <param name="Row">The Row to be written</param>
            /// <param name="Column">The Column to be written</param>
            /// <param name="Value">The Value to be written</param>
            private static void writeCell(Worksheet Worksheet, int Row, int Column, dynamic Value)
            {
                Worksheet.Cells[Row, Column].Value2 = Value;
            }
        }
    }
}
