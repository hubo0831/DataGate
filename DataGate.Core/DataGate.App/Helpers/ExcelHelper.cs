using DataGate.Com;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;

namespace DataGate.App
{
    public class ExcelHelper
    {
        //https://www.cnblogs.com/tanpeng/p/6155749.html
        /// <summary>
        /// 使用EPPlus导出Excel(xlsx)
        /// </summary>
        /// <param name="sourceTable">数据源</param>
        public static Stream ExportByEPPlus(DataTable sourceTable)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                string sheetName = sourceTable.TableName.IsEmpty() ? "Sheet1" : sourceTable.TableName;
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add(sheetName);

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(sourceTable, true);

                //Format the row
                ExcelBorderStyle borderStyle = ExcelBorderStyle.Thin;
                Color borderColor = Color.FromArgb(155, 155, 155);

                using (ExcelRange rng = ws.Cells[1, 1, sourceTable.Rows.Count + 1, sourceTable.Columns.Count])
                {
                    rng.Style.Font.Name = "宋体";
                    rng.Style.Font.Size = 10;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 255));

                    rng.Style.Border.Top.Style = borderStyle;
                    rng.Style.Border.Top.Color.SetColor(borderColor);

                    rng.Style.Border.Bottom.Style = borderStyle;
                    rng.Style.Border.Bottom.Color.SetColor(borderColor);

                    rng.Style.Border.Right.Style = borderStyle;
                    rng.Style.Border.Right.Color.SetColor(borderColor);
                }

                //Format the header row
                using (ExcelRange rng = ws.Cells[1, 1, 1, sourceTable.Columns.Count])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(234, 241, 246));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.FromArgb(51, 51, 51));

                    for (int i = 1; i <= rng.Columns; i++)
                    {
                        int width = CommOp.ToInt(sourceTable.Columns[i - 1].ExtendedProperties["width"]);
                        if (width > 0)
                            ws.Column(i).Width = width / 6;
                        else
                            ws.Column(i).AutoFit(20);
                    }
                    //   rng.AutoFitColumns();
                }


                MemoryStream ms = new MemoryStream(pck.GetAsByteArray());
                return ms;
                //Write it back to the client
                //HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //HttpContext.Current.Response.AddHeader("content-disposition", string.Format("attachment;  filename={0}.xlsx", HttpUtility.UrlEncode(strFileName, Encoding.UTF8)));
                //HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;

                //HttpContext.Current.Response.BinaryWrite(pck.GetAsByteArray());
                //HttpContext.Current.Response.End();
            }
        }
    }
}
