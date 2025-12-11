using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace CommonCollect.Extension
{
    /// <summary>
    /// mini excel包的方法
    /// </summary>
    public sealed class MiniExcelCommon
    {
        public static string DownloadToLocal<T>(List<T> excelData, List<string> headers, string localPath = "")
            where T : class, new()
        {
            string str = "\\excel_" + Guid.NewGuid().ToString().Replace("-", "") + ".xlsx";
            if (string.IsNullOrWhiteSpace(localPath))
            {
                string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadExcel",
                    DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }

                localPath = text;
            }

            FileInfo fileInfo = new FileInfo(localPath + str);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            DataTable dataTable = new DataTable();
            foreach (string header in headers)
            {
                dataTable.Columns.Add(header);
            }

            excelData.ToDataTable(dataTable, hasColumns: false);
            MiniExcel.SaveAs(localPath + str, dataTable);
            return localPath + str;
        }

        public static string DownloadToLocal<T>(List<T> excelData, Dictionary<string, Type> headers,
            string localPath = "") where T : class, new()
        {
            string str = "\\excel_" + Guid.NewGuid().ToString().Replace("-", "") + ".xlsx";
            if (string.IsNullOrWhiteSpace(localPath))
            {
                string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadExcel",
                    DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }

                localPath = text;
            }

            FileInfo fileInfo = new FileInfo(localPath + str);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            DataTable dataTable = new DataTable();
            foreach (KeyValuePair<string, Type> header in headers)
            {
                dataTable.Columns.Add(header.Key, header.Value);
            }

            excelData.ToDataTable(dataTable, hasColumns: false);
            MiniExcel.SaveAs(localPath + str, dataTable);
            return localPath + str;
        }

        public static string DownloadToLocal<T>(Dictionary<string, (List<string>, List<T>)> sheetDatas,
            string localPath = "") where T : class, new()
        {
            string str = "\\excel_" + Guid.NewGuid().ToString().Replace("-", "") + ".xlsx";
            if (string.IsNullOrWhiteSpace(localPath))
            {
                string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadExcel",
                    DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }

                localPath = text;
            }

            FileInfo fileInfo = new FileInfo(localPath + str);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            DataSet dataSet = new DataSet();
            foreach (KeyValuePair<string, (List<string>, List<T>)> sheetData in sheetDatas)
            {
                DataTable dataTable = new DataTable { TableName = sheetData.Key };
                var (list, list2) = sheetData.Value;
                foreach (string item in list)
                {
                    dataTable.Columns.Add(item);
                }

                list2.ToDataTable(dataTable, hasColumns: false);
                dataSet.Tables.Add(dataTable);
            }

            MiniExcel.SaveAs(localPath + str, dataSet);
            return localPath + str;
        }

        public static string DownloadToLocal<T>(Dictionary<string, (Dictionary<string, Type>, List<T>)> sheetDatas,
            string localPath = "") where T : class, new()
        {
            string str = "\\excel_" + Guid.NewGuid().ToString().Replace("-", "") + ".xlsx";
            if (string.IsNullOrWhiteSpace(localPath))
            {
                string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadExcel",
                    DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }

                localPath = text;
            }

            FileInfo fileInfo = new FileInfo(localPath + str);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            DataSet dataSet = new DataSet();
            foreach (KeyValuePair<string, (Dictionary<string, Type>, List<T>)> sheetData in sheetDatas)
            {
                DataTable dataTable = new DataTable { TableName = sheetData.Key };
                var (dictionary, list) = sheetData.Value;
                foreach (KeyValuePair<string, Type> item in dictionary)
                {
                    dataTable.Columns.Add(item.Key, item.Value);
                }

                list.ToDataTable(dataTable, hasColumns: false);
                dataSet.Tables.Add(dataTable);
            }

            MiniExcel.SaveAs(localPath + str, dataSet);
            return localPath + str;
        }

        public static Stream CreateToStream<T>(List<T> excelData, List<string> headers) where T : class, new()
        {
            DataTable dataTable = new DataTable();
            foreach (string header in headers)
            {
                dataTable.Columns.Add(header);
            }

            excelData.ToDataTable(dataTable, hasColumns: false);
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.SaveAs(dataTable, true);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream;
        }

        public static Stream CreateToStream<T>(List<T> excelData, Dictionary<string, Type> headers)
            where T : class, new()
        {
            DataTable dataTable = new DataTable();
            foreach (KeyValuePair<string, Type> header in headers)
            {
                dataTable.Columns.Add(header.Key, header.Value);
            }

            excelData.ToDataTable(dataTable, hasColumns: false);
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.SaveAs(dataTable, true);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream;
        }

        public static Stream CreateToStream<T>(Dictionary<string, (List<string>, List<T>)> sheetDatas)
            where T : class, new()
        {
            MemoryStream memoryStream = new MemoryStream();
            DataSet dataSet = new DataSet();
            foreach (KeyValuePair<string, (List<string>, List<T>)> sheetData in sheetDatas)
            {
                DataTable dataTable = new DataTable { TableName = sheetData.Key };
                var (list, list2) = sheetData.Value;
                foreach (string item in list)
                {
                    dataTable.Columns.Add(item);
                }

                list2.ToDataTable(dataTable, hasColumns: false);
                dataSet.Tables.Add(dataTable);
            }

            memoryStream.SaveAs(dataSet, true, "Sheet1", 0, null);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream;
        }

        public static Stream CreateToStream<T>(Dictionary<string, (Dictionary<string, Type>, List<T>)> sheetDatas)
            where T : class, new()
        {
            MemoryStream memoryStream = new MemoryStream();
            DataSet dataSet = new DataSet();
            foreach (KeyValuePair<string, (Dictionary<string, Type>, List<T>)> sheetData in sheetDatas)
            {
                DataTable dataTable = new DataTable { TableName = sheetData.Key };
                var (dictionary, list) = sheetData.Value;
                foreach (KeyValuePair<string, Type> item in dictionary)
                {
                    dataTable.Columns.Add(item.Key, item.Value);
                }

                list.ToDataTable(dataTable, hasColumns: false);
                dataSet.Tables.Add(dataTable);
            }

            memoryStream.SaveAs(dataSet, true, "Sheet1", 0, null);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream;
        }

        public static byte[] CreateToBytes<T>(List<T> excelData, List<string> headers) where T : class, new()
        {
            DataTable dataTable = new DataTable();
            foreach (string header in headers)
            {
                dataTable.Columns.Add(header);
            }

            excelData.ToDataTable(dataTable, hasColumns: false);
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.SaveAs(dataTable, true, "Sheet1", 0, null);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream.GetBuffer();
        }

        public static byte[] CreateToBytes<T>(List<T> excelData, Dictionary<string, Type> headers)
            where T : class, new()
        {
            DataTable dataTable = new DataTable();
            foreach (KeyValuePair<string, Type> header in headers)
            {
                dataTable.Columns.Add(header.Key, header.Value);
            }

            excelData.ToDataTable(dataTable, hasColumns: false);
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.SaveAs(dataTable, true, "Sheet1", 0, null);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream.GetBuffer();
        }

        public static byte[] CreateToBytes<T>(Dictionary<string, (List<string>, List<T>)> sheetDatas)
            where T : class, new()
        {
            MemoryStream memoryStream = new MemoryStream();
            DataSet dataSet = new DataSet();
            foreach (KeyValuePair<string, (List<string>, List<T>)> sheetData in sheetDatas)
            {
                DataTable dataTable = new DataTable { TableName = sheetData.Key };
                var (list, list2) = sheetData.Value;
                foreach (string item in list)
                {
                    dataTable.Columns.Add(item);
                }

                list2.ToDataTable(dataTable, hasColumns: false);
                dataSet.Tables.Add(dataTable);
            }

            memoryStream.SaveAs(dataSet, true, "Sheet1", 0, null);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream.GetBuffer();
        }

        public static byte[] CreateToBytes<T>(Dictionary<string, (Dictionary<string, Type>, List<T>)> sheetDatas)
            where T : class, new()
        {
            MemoryStream memoryStream = new MemoryStream();
            DataSet dataSet = new DataSet();
            foreach (KeyValuePair<string, (Dictionary<string, Type>, List<T>)> sheetData in sheetDatas)
            {
                DataTable dataTable = new DataTable { TableName = sheetData.Key };
                var (dictionary, list) = sheetData.Value;
                foreach (KeyValuePair<string, Type> item in dictionary)
                {
                    dataTable.Columns.Add(item.Key, item.Value);
                }

                list.ToDataTable(dataTable, hasColumns: false);
                dataSet.Tables.Add(dataTable);
            }

            memoryStream.SaveAs(dataSet, true, "Sheet1", 0, null);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream.GetBuffer();
        }
    }
}