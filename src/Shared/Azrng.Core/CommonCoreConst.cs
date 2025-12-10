using System;
using System.Collections.Generic;

namespace Azrng.Core
{
    /// <summary>
    /// 公共常量
    /// </summary>
    internal class CommonCoreConst
    {
        /// <summary>
        /// 数字到中文的映射字典
        /// </summary>
        internal static readonly Dictionary<char, string> _digitToChinese = new Dictionary<char, string>
                                                                            {
                                                                                { '0', "零" },
                                                                                { '1', "一" },
                                                                                { '2', "二" },
                                                                                { '3', "三" },
                                                                                { '4', "四" },
                                                                                { '5', "五" },
                                                                                { '6', "六" },
                                                                                { '7', "七" },
                                                                                { '8', "八" },
                                                                                { '9', "九" }
                                                                            };

        /// <summary>
        /// 文件格式 key：扩展名 value：格式
        /// </summary>
        internal static readonly IDictionary<string, string> FileFormats =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".gif", "7173" },
                { ".jpg", "255216" },
                { ".jpeg", "255216" },
                { ".png", "13780" },
                { ".bmp", "6677" },
                { ".swf", "6787" },
                { ".flv", "7076" },
                { ".wma", "4838" },
                { ".wav", "8273" },
                { ".amr", "3533" },
                { ".mp4", "00" },
                { ".mp3", "255251" },
                { ".pdf", "3780" },
                { ".txt", "12334" },
                { ".zip", "8297" }
            };

        /// <summary>
        /// 内容后缀名 key:文件扩展名 value 对应的类型
        /// </summary>
        internal static readonly IDictionary<string, string> ContentTypeExtensionsMapping =
            new Dictionary<string, string>
            {
                { ".gif", "image/gif" },
                { ".jpg", "image/jpg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".bmp", "application/x-bmp" },
                { ".mp3", "audio/mp3" },
                { ".wma", "audio/x-ms-wma" },
                { ".wav", "audio/wav" },
                { ".amr", "audio/amr" },
                { ".mp4", "video/mpeg4" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".pdf", "application/pdf" },
                { ".txt", "text/plain" },
                { ".doc", "application/msword" },
                { ".xls", "application/vnd.ms-excel" },
                { ".zip", "aplication/zip" },
                { ".csv", "text/csv" },
                { ".ppt", "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }
            };
    }
}