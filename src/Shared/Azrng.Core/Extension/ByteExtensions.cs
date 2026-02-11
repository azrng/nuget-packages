using Azrng.Core;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// byte扩展
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// 转十六进制
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// 转base64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 将字节数组转为流
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static Stream ToStream(this byte[] bytes)
        {
            return new MemoryStream(bytes);
        }

        /// <summary>
        /// 将byte[]转换成int
        /// </summary>
        /// <param name="data">需要转换成整数的byte数组</param>
        public static int ToInt32(this byte[] data)
        {
            //如果传入的字节数组长度小于4,则返回0
            if (data.Length < 4)
            {
                return 0;
            }

            //如果传入的字节数组长度大于4,需要进行处理
            if (data.Length < 4)
            {
                return 0;
            }

            //创建一个临时缓冲区
            var tempBuffer = new byte[4];

            //将传入的字节数组的前4个字节复制到临时缓冲区
            Buffer.BlockCopy(data, 0, tempBuffer, 0, 4);

            //将临时缓冲区的值转换成整数，并赋给num
            return BitConverter.ToInt32(tempBuffer, 0);
        }

        #region 文件操作

        /// <summary>
        /// 获取字节数组文件后缀
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static string? GetFileSuffix(this byte[] bytes)
        {
            var fileCode = GetFileCode(bytes);
            if(fileCode is null)
                return null;
            var key = CommonCoreConst.FileFormats.First(i => i.Value.Equals(fileCode)).Key;
            return !CommonCoreConst.ContentTypeExtensionsMapping.ContainsKey(key) ? null : key;
        }

        /// <summary>
        /// 获取字节数组内容类型
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static string? GetContentType(this byte[] bytes)
        {
            var fileCode = GetFileCode(bytes);
            var extensions = CommonCoreConst.FileFormats.First((i) => i.Value.Equals(fileCode)).Key;
            return !CommonCoreConst.ContentTypeExtensionsMapping.ContainsKey(extensions)
                ? null
                : CommonCoreConst.ContentTypeExtensionsMapping.Where(x => x.Key == extensions)
                    .Select(x => x.Value)
                    .FirstOrDefault();
        }

        /// <summary>
        /// 获取随机文件名
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns></returns>
        public static string GetRandomFileName(this byte[] data)
        {
            var fileCode = GetFileCode(data);
            return string.Concat(Guid.NewGuid().ToString("n"),
                CommonCoreConst.FileFormats.First(i => i.Value.Equals(fileCode)).Key);
        }


        /// <summary>
        /// 将字节数据保存为文件
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="fileName"></param>
        public static void ToFileByBytes(this byte[] bytes, string fileName)
        {
            using var fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
            using var binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(bytes);
        }

        #endregion

        /// <summary>
        /// 获取文件编码
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        private static string? GetFileCode(byte[] bytes)
        {
            if (bytes.Length < 2)
            {
                return null;
            }
            return bytes[0].ToString(CultureInfo.InvariantCulture) + bytes[1];
        }
    }
}