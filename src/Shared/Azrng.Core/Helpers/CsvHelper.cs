using System;
using System.Data;
using System.IO;
using System.Text;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// CSV文件转换类
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// 导出报表为Csv
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="strFilePath">物理路径</param>
        /// <param name="tableHeader">表头</param>
        /// <param name="columName">字段标题,逗号分隔</param>
        public static bool DateTableToCsv(DataTable dt, string strFilePath, string tableHeader, string columName)
        {
            try
            {
                var strmWriterObj = new StreamWriter(strFilePath, false, Encoding.UTF8);
                strmWriterObj.WriteLine(tableHeader);
                strmWriterObj.WriteLine(columName);
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    var strBufferLine = "";
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j > 0)
                            strBufferLine += ",";
                        strBufferLine += dt.Rows[i][j].ToString();
                    }

                    strmWriterObj.WriteLine(strBufferLine);
                }

                strmWriterObj.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将Csv读入DataTable
        /// </summary>
        /// <param name="fileStream">csv文件路径</param>
        /// <param name="n">表示第n行是字段title,第n+1行是记录开始</param>
        public static DataTable CsvToDateTable(Stream fileStream, int n)
        {
            var dt = new DataTable();
            var reader = new StreamReader(fileStream, Encoding.UTF8, false);
            var m = 0;
            var columnsInitialized = false;
            reader.Peek();
            while (reader.Peek() > 0)
            {
                var str = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(str))
                    continue;

                m += 1;
                if (m == n)
                {
                    // 当前行为字段标题行，初始化列
                    var headers = str.Split(',');
                    foreach (var header in headers)
                    {
                        dt.Columns.Add(header.Trim(), typeof(string));
                    }
                    columnsInitialized = true;
                }
                else if (m >= n + 1)
                {
                    // 数据行
                    if (!columnsInitialized)
                    {
                        // 如果没有标题行，根据第一行数据初始化列
                        var split = str.Split(',');
                        for (int i = 0; i < split.Length; i++)
                        {
                            dt.Columns.Add($"Column{i + 1}", typeof(string));
                        }
                        columnsInitialized = true;
                    }

                    var dataSplit = str.Split(',');
                    var dr = dt.NewRow();
                    for (int i = 0; i < Math.Min(dataSplit.Length, dt.Columns.Count); i++)
                    {
                        dr[i] = dataSplit[i];
                    }

                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }
    }
}