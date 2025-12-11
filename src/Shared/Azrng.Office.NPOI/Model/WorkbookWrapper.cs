using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Azrng.Office.NPOI.Model
{
    /// <summary>
    /// IWorkbook包装
    /// </summary>
    public class WorkbookWrapper
    {
        public WorkbookWrapper(IWorkbook workbook)
        {
            Workbook = workbook;
        }

        public WorkbookWrapper(ExcelFileType ext)
        {
            Workbook = ext == ExcelFileType.Xls ? new HSSFWorkbook() : new XSSFWorkbook();
        }

        public IWorkbook Workbook { get; private set; }

        /// <summary>
        /// 工作簿转流
        /// </summary>
        /// <returns></returns>
        public Stream ToStream()
        {
            var stream = new MemoryStream();
            Workbook.Write(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// 工作簿写入流
        /// </summary>
        /// <returns></returns>
        public void WriteStream(FileStream stream)
        {
            try
            {
                Workbook.Write(stream);
            }
            finally
            {
                Workbook?.Close();
            }
        }

        /// <summary>
        /// 导出到文件
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveToFile(string filePath)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                Workbook.Write(fileStream);
            }
            finally
            {
                Workbook?.Close();
            }
        }

        /// <summary>
        /// 工作簿转字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            try
            {
                using var stream = new MemoryStream();
                Workbook.Write(stream);
                return stream.ToArray();
            }
            finally
            {
                Workbook?.Close();
            }
        }
    }
}