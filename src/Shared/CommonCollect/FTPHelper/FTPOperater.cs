using System;
using System.IO;
using System.Text;

namespace CommonCollect.FTPHelper.FTPHelper
{
    /// <summary>
    /// FTP操作类
    /// </summary>
    public class FTPOperater
    {
        #region 属性

        /// <summary>
        /// 全局FTP访问变量
        /// </summary>
        public FTPClient Ftp { get; set; }

        /// <summary>
        /// Ftp服务器
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Ftp用户
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Ftp密码
        /// </summary>
        public string Pass { get; set; }

        /// <summary>
        /// Ftp密码
        /// </summary>
        public string FolderZJ { get; set; }

        /// <summary>
        /// Ftp密码
        /// </summary>
        public string FolderWX { get; set; }

        #endregion

        /// <summary>
        /// 得到文件列表
        /// </summary>
        /// <returns></returns>
        public string[] GetList(string strPath)
        {
            if (Ftp == null) Ftp = getFtpClient();
            Ftp.Connect();
            Ftp.ChDir(strPath);
            return Ftp.Dir("*");
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="FtpFolder">Ftp目录</param>
        /// <param name="FtpFileName">Ftp文件名</param>
        /// <param name="localFolder">本地目录</param>
        /// <param name="localFileName">本地文件名</param>
        public bool GetFile(string FtpFolder, string FtpFileName, string localFolder, string localFileName)
        {
            try
            {
                if (Ftp == null) Ftp = getFtpClient();
                if (!Ftp.Connected)
                {
                    Ftp.Connect();
                    Ftp.ChDir(FtpFolder);
                }
                Ftp.Get(FtpFileName, localFolder, localFileName);

                return true;
            }
            catch
            {
                try
                {
                    Ftp.DisConnect();
                    Ftp = null;
                }
                catch { Ftp = null; }
                return false;
            }
        }

        /// <summary>
        /// 修改文件
        /// </summary>
        /// <param name="FtpFolder">本地目录</param>
        /// <param name="FtpFileName">本地文件名temp</param>
        /// <param name="localFolder">本地目录</param>
        /// <param name="localFileName">本地文件名</param>
        public bool AddMSCFile(string FtpFolder, string FtpFileName, string localFolder, string localFileName, string BscInfo)
        {
            string sLine = "";
            string sResult = "";
            string path = "获得应用程序所在的完整的路径";
            path = path.Substring(0, path.LastIndexOf("\\"));
            try
            {
                FileStream fsFile = new FileStream(FtpFolder + "\\" + FtpFileName, FileMode.Open);
                FileStream fsFileWrite = new FileStream(localFolder + "\\" + localFileName, FileMode.Create);
                StreamReader sr = new StreamReader(fsFile);
                StreamWriter sw = new StreamWriter(fsFileWrite);
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                while (sr.Peek() > -1)
                {
                    sLine = sr.ReadToEnd();
                }
                string[] arStr = sLine.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < arStr.Length - 1; i++)
                {
                    sResult += BscInfo + "," + arStr[i].Trim() + "\n";
                }
                sr.Close();
                byte[] connect = new UTF8Encoding(true).GetBytes(sResult);
                fsFileWrite.Write(connect, 0, connect.Length);
                fsFileWrite.Flush();
                sw.Close();
                fsFile.Close();
                fsFileWrite.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="FtpFolder">Ftp目录</param>
        /// <param name="FtpFileName">Ftp文件名</param>
        public bool DelFile(string FtpFolder, string FtpFileName)
        {
            try
            {
                if (Ftp == null) Ftp = getFtpClient();
                if (!Ftp.Connected)
                {
                    Ftp.Connect();
                    Ftp.ChDir(FtpFolder);
                }
                Ftp.Delete(FtpFileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="FtpFolder">Ftp目录</param>
        /// <param name="FtpFileName">Ftp文件名</param>
        public bool PutFile(string FtpFolder, string FtpFileName)
        {
            try
            {
                if (Ftp == null) Ftp = getFtpClient();
                if (!Ftp.Connected)
                {
                    Ftp.Connect();
                    Ftp.ChDir(FtpFolder);
                }
                Ftp.Put(FtpFileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="FtpFolder">Ftp目录</param>
        /// <param name="FtpFileName">Ftp文件名</param>
        /// <param name="localFolder">本地目录</param>
        /// <param name="localFileName">本地文件名</param>
        public bool GetFileNoBinary(string FtpFolder, string FtpFileName, string localFolder, string localFileName)
        {
            try
            {
                if (Ftp == null) Ftp = getFtpClient();
                if (!Ftp.Connected)
                {
                    Ftp.Connect();
                    Ftp.ChDir(FtpFolder);
                }
                Ftp.GetNoBinary(FtpFileName, localFolder, localFileName);
                return true;
            }
            catch
            {
                try
                {
                    Ftp.DisConnect();
                    Ftp = null;
                }
                catch
                {
                    Ftp = null;
                }
                return false;
            }
        }

        /// <summary>
        /// 得到FTP上文件信息
        /// </summary>
        /// <param name="FtpFolder">FTP目录</param>
        /// <param name="FtpFileName">Ftp文件名</param>
        public string GetFileInfo(string FtpFolder, string FtpFileName)
        {
            string strResult = "";
            try
            {
                if (Ftp == null) Ftp = getFtpClient();
                if (Ftp.Connected) Ftp.DisConnect();
                Ftp.Connect();
                Ftp.ChDir(FtpFolder);
                strResult = Ftp.GetFileInfo(FtpFileName);
                return strResult;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 测试FTP服务器是否可登陆
        /// </summary>
        public bool CanConnect()
        {
            if (Ftp == null) Ftp = getFtpClient();
            try
            {
                Ftp.Connect();
                Ftp.DisConnect();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 得到FTP上文件信息
        /// </summary>
        /// <param name="FtpFolder">FTP目录</param>
        /// <param name="FtpFileName">Ftp文件名</param>
        public string GetFileInfoConnected(string FtpFolder, string FtpFileName)
        {
            string strResult = "";
            try
            {
                if (Ftp == null) Ftp = getFtpClient();
                if (!Ftp.Connected)
                {
                    Ftp.Connect();
                    Ftp.ChDir(FtpFolder);
                }
                strResult = Ftp.GetFileInfo(FtpFileName);
                return strResult;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 得到文件列表
        /// </summary>
        /// <param name="FtpFolder">FTP目录</param>
        /// <returns>FTP通配符号</returns>
        public string[] GetFileList(string FtpFolder, string strMask)
        {
            string[] strResult;
            try
            {
                if (Ftp == null) Ftp = getFtpClient();
                if (!Ftp.Connected)
                {
                    Ftp.Connect();
                    Ftp.ChDir(FtpFolder);
                }
                strResult = Ftp.Dir(strMask);
                return strResult;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///得到FTP传输对象
        /// </summary>
        public FTPClient getFtpClient()
        {
            FTPClient ft = new FTPClient();
            ft.RemoteHost = Server;
            ft.RemoteUser = User;
            ft.RemotePass = Pass;
            return ft;
        }
    }
}