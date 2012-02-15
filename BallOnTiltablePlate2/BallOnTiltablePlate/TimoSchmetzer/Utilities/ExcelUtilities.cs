using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace BallOnTiltablePlate.TimoSchmetzer.Utilities
{
    class ExcelUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data">Tuple<IEnumerable<headings>,Ienumerable<Tuple<xData,Ienumerable<yData>>>></param>
        public static void GenerateAndShowDiagram(Tuple<IEnumerable<string>, IEnumerable<Tuple<double, IEnumerable<double>>>> Data)
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
            #region WriteValues
            int[] WritePosition = new int[2];
            WritePosition[0] = 1;
            WritePosition[1] = 1;
            //Generate headings
            foreach (string heading in Data.Item1)
            {
                writeCell(ws, WritePosition[0], WritePosition[1], heading);
                WritePosition[1]++;
            }
            //Write Cell values
            foreach (Tuple<double, IEnumerable<double>> CellValues in Data.Item2)
            {
                WritePosition[0]++;
                writeCell(ws, WritePosition[0], WritePosition[1], CellValues.Item1);
                foreach (double yValue in CellValues.Item2)
                {
                    WritePosition[1]++;
                    writeCell(ws, WritePosition[0], WritePosition[1], yValue);
                }
            }
            #endregion
            #region CreateDiagram
            Chart c = wb.Charts.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            c.ChartWizard(Missing.Value, XlChartType.xlXYScatter, Missing.Value, XlRowCol.xlColumns, Missing.Value, Missing.Value, Missing.Value,
            Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            Series s = c.SeriesCollection(1);
            int DataLength = Data.Item2.ToArray().Length;
            s.XValues = ws.get_Range(ws.Cells[2, 1], ws.Cells[DataLength, 1]);
            s.Values = ws.get_Range(ws.Cells[2, 2], ws.Cells[DataLength, 2]);
            s.Name = Data.Item1.ElementAt(0);
            c.Location(XlChartLocation.xlLocationAsNewSheet, ws.Name);
            #endregion
        }

        public static void writeCell(Worksheet Worksheet, int Row, int Column, dynamic Value)
        {
            Worksheet.Cells[Row, Column].Value2 = Value;
        }
    }
}
