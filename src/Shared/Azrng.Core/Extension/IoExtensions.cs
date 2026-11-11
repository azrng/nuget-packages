using System.IO;
using System.Threading.Tasks;
using System;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// io相关的扩展
    /// </summary>
    public static class IoExtensions
    {
        /// <summary>
        /// 将指定文件转流
        /// </summary>
        /// <param name="fileName">文件所在地址</param>
        /// <returns></returns>
        public static Stream? ToStreamFromFile(this string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var array = new byte[fileStream.Length];
            _ = fileStream.Read(array, 0, array.Length);
            return new MemoryStream(array);
        }

        /// <summary>
        /// 将指定文件转流
        /// </summary>
        /// <param name="fileName">文件所在地址</param>
        /// <returns></returns>
        public static async Task<Stream?> ToStreamFromFileAsync(this string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            await using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var array = new byte[fileStream.Length];
            _ = await fileStream.ReadAsync(array).ConfigureAwait(false);
            return new MemoryStream(array);
        }

        /// <summary>
        /// 将指定文件转字节数组
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[]? ToBytesFromFile(this string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var array = new byte[fileStream.Length];
            _ = fileStream.Read(array, 0, array.Length);
            return array;
        }

        /// <summary>
        /// 将指定文件转字节数组
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<byte[]?>? ToBytesFromFileAsync(this string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            await using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var array = new byte[fileStream.Length];
            _ = await fileStream.ReadAsync(array).ConfigureAwait(false);
            return array;
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="fileName">文件地址</param>
        /// <returns></returns>
        public static long? GetFileSize(this string fileName)
        {
            return File.Exists(fileName) ? new FileInfo(fileName).Length : 0;
        }

        /// <summary>
        /// 获取文件格式化的大小
        /// </summary>
        /// <param name="fileName">文件地址</param>
        /// <returns></returns>
        public static string? GetFileFormatSize(this string fileName)
        {
            var factSize = fileName.GetFileSize();
            if (factSize is null)
                return null;
            var formatResult = string.Empty;
            if (factSize < 1024.00)
                formatResult = factSize.Value.ToString("F2") + " Byte";
            else if (factSize >= 1024.00 && factSize < 1048576)
                formatResult = (factSize / 1024.00).Value.ToString("F2") + " K";
            else if (factSize is >= 1048576 and < 1073741824)
                formatResult = (factSize / 1024.00 / 1024.00).Value.ToString("F2") + " M";
            else if (factSize >= 1073741824)
                formatResult = (factSize / 1024.00 / 1024.00 / 1024.00).Value.ToString("F2") + " G";
            return formatResult;
        }

        /// <summary>
        /// 将流转字节数组
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[]? ToBytes(this Stream? stream)
        {
            if (stream is null || stream.Length == 0)
                return Array.Empty<byte>();

            var array = new byte[stream.Length];
            ReadExactly(stream, array);
            stream.Seek(0L, SeekOrigin.Begin);

            return array;
        }

        /// <summary>
        /// 将流转字节数组
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<byte[]>? ToBytesAsync(this Stream? stream)
        {
            if (stream is null || stream.Length == 0)
                return Array.Empty<byte>();
            var array = new byte[stream.Length];
            await ReadExactlyAsync(stream, array);
            stream.Seek(0L, SeekOrigin.Begin);
            return array;
        }

        /// <summary>
        /// 将指定流保存为文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="filePath">文件地址</param>
        public static bool ToFile(this Stream? stream, string filePath)
        {
            if (stream is null || stream.Length == 0)
                return false;
            var array = new byte[stream.Length];
            ReadExactly(stream, array);
            stream.Seek(0L, SeekOrigin.Begin);
            //FileMode.Create 会截断已存在文件，避免新内容比旧内容短时尾部残留旧字节
            using var fileStream = new FileStream(filePath, FileMode.Create);
            using var binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(array);
            return true;
        }

        /// <summary>
        /// 将指定流保存为文件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filePath"></param>
        public static async Task<bool> ToFileAsync(this Stream? stream, string filePath)
        {
            if (stream is null || stream.Length == 0)
                return false;

            var array = new byte[stream.Length];
            await ReadExactlyAsync(stream, array).ConfigureAwait(false);
            stream.Seek(0L, SeekOrigin.Begin);
            //FileMode.Create 会截断已存在文件，避免新内容比旧内容短时尾部残留旧字节
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await using var binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(array);
            return true;
        }

        /// <summary>
        /// 循环读满缓冲区：Stream.Read 允许部分读，网络流等场景单次 Read 可能截断
        /// </summary>
        private static void ReadExactly(Stream stream, byte[] buffer)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var read = stream.Read(buffer, offset, buffer.Length - offset);
                if (read <= 0)
                    throw new EndOfStreamException($"流在读取 {offset}/{buffer.Length} 字节后提前结束。");
                offset += read;
            }
        }

        /// <summary>
        /// 循环读满缓冲区：Stream.Read 允许部分读，网络流等场景单次 Read 可能截断
        /// </summary>
        private static async Task ReadExactlyAsync(Stream stream, byte[] buffer)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer, offset, buffer.Length - offset);
                if (read <= 0)
                    throw new EndOfStreamException($"流在读取 {offset}/{buffer.Length} 字节后提前结束。");
                offset += read;
            }
        }
    }
}