using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CommonCollect.FileHelper
{
    public class FileHelper2 : IDisposable
    {
        #region 获取指定目录下的文件名称

        /// <summary>
        /// 获取指定目录下的文件名称
        /// </summary>
        /// <param name="path">绝对路径</param>
        /// <param name="type">类别  例如：*.txt</param>
        /// <returns></returns>
        public string[] GetFiles(string path, string type)
        {
            return Directory.GetFiles(path, type);
        }

        #endregion 获取指定目录下的文件名称

        private bool _alreadyDispose = false;

        #region 构造函数

        public FileHelper2()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        ~FileHelper2()
        {
            Dispose(); ;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_alreadyDispose) return;
            _alreadyDispose = true;
        }

        #endregion 构造函数

        #region IDisposable 成员

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable 成员

        #region 取得文件后缀名

        /****************************************
          * 函数名称：GetPostfixStr
          * 功能说明：取得文件后缀名
          * 参     数：filename:文件名称
          * 调用示列：
          *            string filename = "aaa.aspx";
          *            string s = EC.FileObj.GetPostfixStr(filename);
         *****************************************/

        /// <summary>
        /// 取后缀名
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>.gif|.html格式</returns>
        public static string GetPostfixStr(string filename)
        {
            int start = filename.LastIndexOf(".");
            int length = filename.Length;
            string postfix = filename.Substring(start, length - start);
            return postfix;
        }

        #endregion 取得文件后缀名

        #region 写文件

        /****************************************
          * 函数名称：WriteFile
          * 功能说明：写文件,会覆盖掉以前的内容
          * 参     数：Path:文件路径,Strings:文本内容
          * 调用示列：
          *            string Path = Server.MapPath("Default2.aspx");
          *            string Strings = "这是我写的内容啊";
          *            EC.FileObj.WriteFile(Path,Strings);
         *****************************************/

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="Strings">文件内容</param>
        public static void WriteFile(string Path, string Strings)
        {
            if (!File.Exists(Path))
            {
                FileStream f = File.Create(Path);
                f.Close();
            }
            StreamWriter f2 = new StreamWriter(Path, false, Encoding.GetEncoding("gb2312"));
            f2.Write(Strings);
            f2.Close();
            f2.Dispose();
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="Strings">文件内容</param>
        /// <param name="encode">编码格式</param>
        public static void WriteFile(string Path, string Strings, Encoding encode)
        {
            if (!File.Exists(Path))
            {
                FileStream f = File.Create(Path);
                f.Close();
            }
            StreamWriter f2 = new StreamWriter(Path, false, encode);
            f2.Write(Strings);
            f2.Close();
            f2.Dispose();
        }

        #endregion 写文件

        #region 读文件

        /****************************************
          * 函数名称：ReadFile
          * 功能说明：读取文本内容
          * 参     数：Path:文件路径
          * 调用示列：
          *            string Path = Server.MapPath("Default2.aspx");
          *            string s = EC.FileObj.ReadFile(Path);
         *****************************************/

        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <returns></returns>
        public static string ReadFile(string Path)
        {
            string s = "";
            if (!File.Exists(Path))
                s = "不存在相应的目录";
            else
            {
                StreamReader f2 = new StreamReader(Path, Encoding.GetEncoding("gb2312"));
                s = f2.ReadToEnd();
                f2.Close();
                f2.Dispose();
            }

            return s;
        }

        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="encode">编码格式</param>
        /// <returns></returns>
        public static string ReadFile(string Path, Encoding encode)
        {
            string s = "";
            if (!File.Exists(Path))
                s = "不存在相应的目录";
            else
            {
                StreamReader f2 = new StreamReader(Path, encode);
                s = f2.ReadToEnd();
                f2.Close();
                f2.Dispose();
            }

            return s;
        }

        #endregion 读文件

        #region 追加文件

        /****************************************
          * 函数名称：FileAdd
          * 功能说明：追加文件内容
          * 参     数：Path:文件路径,strings:内容
          * 调用示列：
          *            string Path = Server.MapPath("Default2.aspx");
          *            string Strings = "新追加内容";
          *            EC.FileObj.FileAdd(Path, Strings);
         *****************************************/

        /// <summary>
        /// 追加文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="strings">内容</param>
        public static void FileAdd(string Path, string strings)
        {
            StreamWriter sw = File.AppendText(Path);
            sw.Write(strings);
            sw.Flush();
            sw.Close();
        }

        #endregion 追加文件

        #region 拷贝文件

        /****************************************
          * 函数名称：FileCoppy
          * 功能说明：拷贝文件
          * 参     数：OrignFile:原始文件,NewFile:新文件路径
          * 调用示列：
          *            string orignFile = Server.MapPath("Default2.aspx");
          *            string NewFile = Server.MapPath("Default3.aspx");
          *            EC.FileObj.FileCoppy(OrignFile, NewFile);
         *****************************************/

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="OrignFile">原始文件</param>
        /// <param name="NewFile">新文件路径</param>
        public static void FileCoppy(string orignFile, string NewFile)
        {
            File.Copy(orignFile, NewFile, true);
        }

        #endregion 拷贝文件

        #region 删除文件

        /****************************************
          * 函数名称：FileDel
          * 功能说明：删除文件
          * 参     数：Path:文件路径
          * 调用示列：
          *            string Path = Server.MapPath("Default3.aspx");
          *            EC.FileObj.FileDel(Path);
         *****************************************/

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="Path">路径</param>
        public static void FileDel(string Path)
        {
            File.Delete(Path);
        }

        #endregion 删除文件

        #region 移动文件

        /****************************************
          * 函数名称：FileMove
          * 功能说明：移动文件
          * 参     数：OrignFile:原始路径,NewFile:新文件路径
          * 调用示列：
          *             string orignFile = Server.MapPath("../说明.txt");
          *             string NewFile = Server.MapPath("http://www.cnHyds.com/说明.txt");
          *             EC.FileObj.FileMove(OrignFile, NewFile);
         *****************************************/

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="OrignFile">原始路径</param>
        /// <param name="NewFile">新路径</param>
        public static void FileMove(string orignFile, string NewFile)
        {
            File.Move(orignFile, NewFile);
        }

        #endregion 移动文件

        #region 在当前目录下创建目录

        /****************************************
          * 函数名称：FolderCreate
          * 功能说明：在当前目录下创建目录
          * 参     数：OrignFolder:当前目录,NewFloder:新目录
          * 调用示列：
          *            string orignFolder = Server.MapPath("test/");
          *            string NewFloder = "new";
          *            EC.FileObj.FolderCreate(OrignFolder, NewFloder);
         *****************************************/

        /// <summary>
        /// 在当前目录下创建目录
        /// </summary>
        /// <param name="OrignFolder">当前目录</param>
        /// <param name="NewFloder">新目录</param>
        public static void FolderCreate(string orignFolder, string NewFloder)
        {
            Directory.SetCurrentDirectory(orignFolder);
            Directory.CreateDirectory(NewFloder);
        }

        #endregion 在当前目录下创建目录

        #region 递归删除文件夹目录及文件

        /****************************************
          * 函数名称：DeleteFolder
          * 功能说明：递归删除文件夹目录及文件
          * 参     数：dir:文件夹路径
          * 调用示列：
          *            string dir = Server.MapPath("test/");
          *            EC.FileObj.DeleteFolder(dir);
         *****************************************/

        /// <summary>
        /// 递归删除文件夹目录及文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static void DeleteFolder(string dir)
        {
            if (Directory.Exists(dir)) //如果存在这个文件夹删除之
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                        File.Delete(d); //直接删除其中的文件
                    else
                        DeleteFolder(d); //递归删除子文件夹
                }
                Directory.Delete(dir); //删除已空文件夹
            }
        }

        #endregion 递归删除文件夹目录及文件



        #region 相对路径转绝对路径

        /// <summary>
        /// 相对路径转绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(string path)
        {
            return Directory.GetCurrentDirectory() + path;
        }

        #endregion 相对路径转绝对路径

        /// <summary>
        /// 文件流加密
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public static string GetCryptoString(Stream fileStream)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] cryptBytes = md5.ComputeHash(fileStream);
            return GetCryptoString(cryptBytes);
        }

        /// <summary>
        /// 文件流加密
        /// </summary>
        /// <param name="cryptBytes"></param>
        /// <returns></returns>
        public static string GetCryptoString(byte[] cryptBytes)
        {
            //加密的二进制转为string类型返回
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cryptBytes.Length; i++)
            {
                sb.Append(cryptBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}