using Azrng.Core.Exceptions;
using Azrng.Core.Extension;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// IO帮助类  来自 :https://github.com/linys2333/Lys.NetExtend/blob/dotNetStandard/AnyExtend/IOExt.cs
    /// </summary>
    public class IOHelper
    {
        /// <summary>
        /// 存放用户数据的公共文件夹路径
        /// </summary>
        public static readonly string ApplicationDataPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// 获取桌面文件路径
        /// </summary>
        public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        /// <summary>
        /// 去只读
        /// <para>IOFilePathException异常</para>
        /// <para>IOFolderPathException异常</para>
        /// </summary>
        /// <param name="path">文件或文件夹路径</param>
        /// <param name="isFile">类型是文件还是文件夹</param>
        public static void RemoveReadonly(string path, bool isFile = true)
        {
            // 文件
            if (isFile)
            {
                if (!CheckFilePath(path))
                {
                    throw new ParameterException(path);
                }

                if (File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                }
            }
            // 文件夹
            else
            {
                if (!CheckFolderPath(path, false))
                {
                    throw new ParameterException(path);
                }

                if (Directory.Exists(path))
                {
                    var folder = new DirectoryInfo(path);
                    folder.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                }
            }
        }

        /// <summary>
        /// 校验文件扩展名是否合法
        /// </summary>
        /// <param name="ext">扩展名（不含.）</param>
        /// <returns></returns>
        public static bool CheckExt(string ext)
        {
            return Regex.IsMatch(ext, RegexExtensions.ExtName);
        }

        /// <summary>
        /// 校验文件夹路径是否合法，示例：
        /// <para>C:</para>
        /// <para>C:\</para>
        /// <para>C:\folder</para>
        /// <para>C:\folder\</para>
        /// <para>C:\folder\1.txt</para>
        /// </summary>
        /// <param name="path">需校验的完整路径</param>
        /// <param name="isAvailDiskRootPath">磁盘根路径是否有效</param>
        /// <returns></returns>
        public static bool CheckFolderPath(string path, bool isAvailDiskRootPath = true)
        {
            var pattern = isAvailDiskRootPath ? RegexExtensions.FolderPath : RegexExtensions.FolderPath2;

            return Regex.IsMatch(path, pattern);
        }

        /// <summary>
        /// 校验文件路径是否合法，示例：
        /// <para>C:\1.txt</para>
        /// <para>C:\folder\1.txt</para>
        /// <para>C:\folder\1txt</para>
        /// <para>IOExtNameException异常</para>
        /// </summary>
        /// <param name="path">需校验的完整路径</param>
        /// <param name="ext">扩展名（不含.）
        /// <para>若ext=""，则不校验扩展名</para>
        /// <para>若ext=*，则校验任意扩展名</para>
        /// <para>否则，校验指定扩展名</para>
        /// </param>
        /// <returns></returns>
        public static bool CheckFilePath(string path, string ext = "")
        {
            // 解析：[盘符:] + 1或多段[\合法文件夹或文件名]
            string pattern;
            switch (ext)
            {
                case "":
                    pattern = RegexExtensions.FilePath;
                    break;

                case "*":
                    pattern = RegexExtensions.FilePath2;
                    break;

                default:
                    if (!CheckExt(ext))
                    {
                        throw new ParameterException(ext);
                    }

                    pattern =
                        $@"^{RegexExtensions.AvailDisk}:({RegexExtensions.AvailSplit}[^{RegexExtensions.NoAvailChar}]+)+(?<=\.{ext.ToRegex()})$";
                    break;
            }

            return path.IsMatch(pattern);
        }

        /// <summary>
        /// 创建文件夹，若存在则无处理
        /// <para>IOFolderPathException异常</para>
        /// </summary>
        /// <param name="path">文件夹路径</param>
        public static void CreateFolder(string path)
        {
            if (!CheckFolderPath(path, false))
            {
                throw new ParameterException(path);
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 根据文件路径获取文件夹路径（以 \ 结尾）
        /// <para>IOFilePathException异常</para>
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static string GetFolder(string path)
        {
            if (!CheckFilePath(path))
            {
                throw new ParameterException(path);
            }

            var length = path.LastIndexOf('\\') + 1;

            // 左截取
            length = path.Length < Math.Abs(length) ? 0 : length;
            length = length <= 0 ? path.Length + length : length;
            return path.Substring(0, length);
        }

        /// <summary>
        /// 检查文件是否被其他进程锁定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                // 文件未被锁定
                return false;
            }
            catch (IOException)
            {
                // 文件被锁定
                return true;
            }
        }

        #region 操作文件

        #region XML操作

        /// <summary>
        /// 写入xml文本。若文件不存在，则先创建
        /// <para>IOFilePathException异常</para>
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="xmlDoc">xml对象</param>
        public static void WriteXml(string path, XmlDocument xmlDoc)
        {
            CreateXml(path, xmlDoc);

            // 去只读
            RemoveReadonly(path);

            xmlDoc.Save(path);
        }

        /// <summary>
        /// 创建xml文件
        /// <para>IOFilePathException异常</para>
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="xmlDoc">xml对象</param>
        public static void CreateXml(string path, XmlDocument xmlDoc)
        {
            if (!CheckFilePath(path))
            {
                throw new ParameterException(path);
            }

            if (!File.Exists(path))
            {
                CreateFolder(GetFolder(path));
                xmlDoc.Save(path);
            }
        }

        /// <summary>
        /// 创建xml文件（UTF8格式）
        /// <para>IOFilePathException异常</para>
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="rootNodeName">根节点名称</param>
        public static XmlDocument CreateXml(string path, string rootNodeName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));
            xmlDoc.AppendChild(xmlDoc.CreateElement(rootNodeName));

            CreateXml(path, xmlDoc);
            return xmlDoc;
        }

        /// <summary>
        /// 读取xml文本
        /// <para>IOFilePathException异常</para>
        /// <para>IOException</para>
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static XmlDocument ReadXml(string path)
        {
            if (!CheckFilePath(path))
            {
                throw new ParameterException(path);
            }

            if (!File.Exists(path))
            {
                throw new IOException($"文件路径【{path}】不存在！");
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            return xmlDoc;
        }

        #endregion XML操作

        #endregion 操作文件
    }
}