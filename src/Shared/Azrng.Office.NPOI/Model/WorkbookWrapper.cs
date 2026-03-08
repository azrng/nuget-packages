using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Azrng.Office.NPOI;

namespace Azrng.Office.NPOI.Model
{
    /// <summary>
    /// IWorkbook包装
    /// </summary>
    public class WorkbookWrapper : IDisposable
    {
        private bool _disposed;

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
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Workbook?.Close();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~WorkbookWrapper()
        {
            Dispose(false);
        }

        /// <summary>
        /// 工作簿转流
        /// 注意：调用者负责关闭返回的流
        /// </summary>
        /// <returns></returns>
        public Stream ToStream()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkbookWrapper));

            var stream = new MemoryStream();
            try
            {
                Workbook.Write(stream);
                stream.Position = 0;
                return stream;
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 工作簿写入流
        /// </summary>
        /// <param name="stream">文件流</param>
        public void WriteStream(FileStream stream)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkbookWrapper));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                Workbook.Write(stream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to write workbook to stream", ex);
            }
        }

        /// <summary>
        /// 导出到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void SaveToFile(string filePath)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkbookWrapper));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            try
            {
                // 规范化并验证路径
                var normalizedPath = Path.GetFullPath(filePath);
                var directory = Path.GetDirectoryName(normalizedPath);

                // 验证目录是否存在
                if (!Directory.Exists(directory))
                    throw new DirectoryNotFoundException($"Directory not found: {directory}");

                // 验证文件扩展名
                var extension = Path.GetExtension(normalizedPath).ToLowerInvariant();
                if (extension != ".xls" && extension != ".xlsx")
                    throw new ArgumentException("File must be .xls or .xlsx format", nameof(filePath));

                using var fileStream = new FileStream(normalizedPath, FileMode.Create, FileAccess.Write);
                Workbook.Write(fileStream);
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not DirectoryNotFoundException)
            {
                throw new InvalidOperationException("Failed to save workbook to file", ex);
            }
        }

        /// <summary>
        /// 工作簿转字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        public byte[] ToBytes()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkbookWrapper));

            using var stream = new MemoryStream();
            Workbook.Write(stream);
            return stream.ToArray();
        }
    }
}