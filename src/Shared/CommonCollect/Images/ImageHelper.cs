using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CommonCollect.FileHelper
{
    public class ImageHelper
    {
        //是否已经加载了JPEG编码解码器
        private static bool _isloadjpegcodec = false;
        //当前系统安装的JPEG编码解码器
        private static ImageCodecInfo _jpegcodec = null;

        #region 相对路径转绝对路径
        /// <summary>
        /// 相对路径转绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetAbsolutePath(string path)
        {
            return Directory.GetCurrentDirectory() + path;
        }
        #endregion



        #region 将指定路径图片转换格式

        #region 将指定路径图片转byte数组
        public static byte[] ImagePathToByte(string path)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    int fileLength = 0;
                    fileLength = (int)fs.Length;
                    byte[] imageByte = new byte[fileLength];
                    fs.Read(imageByte, 0, fileLength);
                    fs.Close();
                    return imageByte;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }
        #endregion

        #region 网络图片转换byte[]
        /// <summary>
        /// 网络图片转换byte[]
        /// </summary>
        /// <param name="fileUrl">附带http开头的全路径</param>
        /// <returns></returns>
        public static byte[] URLImageToByte(string FileUrl)
        {
            byte[] fileBytes = new byte[1];

            if (!string.IsNullOrEmpty(FileUrl))
            {
                Uri mUri = new Uri(FileUrl);
                HttpWebRequest mRequest = (HttpWebRequest)WebRequest.Create(mUri);
                mRequest.Method = "GET";
                mRequest.Timeout = 200;
                mRequest.ContentType = "text/html;charset=utf-8";

                HttpWebResponse mResponse = (HttpWebResponse)mRequest.GetResponse();
                if (mResponse.StatusCode == HttpStatusCode.OK)
                {
                    Stream mStream = mResponse.GetResponseStream();
                    fileBytes = new byte[mStream.Length];
                    mStream.Read(fileBytes, 0, Convert.ToInt32(mStream.Length));
                }
            }
            return fileBytes;
        }
        #endregion

        #region 将指定路径图片转换为base64字符串
        /// <summary>
        /// 将指定路径图片转换为base64字符串
        /// </summary>
        /// <param name="path">图片相对路径</param>
        /// <returns>base64 string result</returns>
        public static string PathImageToBase64(string path)
        {
            try
            {
                var byteArr = ImagePathToByte(path);
                return Convert.ToBase64String(byteArr);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #endregion

        #region 将Image格式转换为其他格式

        #region 将Image格式转换为byte[]
        /// <summary>
        /// 将Image格式转换为byte[]
        /// </summary>
        /// <param name="image">图片</param>
        /// <returns></returns>
        public byte[] ImageToByte(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, image);
                return ms.ToArray();
            }
        }
        #endregion



        #endregion

        #region 将其他格式转成image

        #region 将byte格式[]转换为Image
        /// <summary>
        /// 将byte[]格式转换为Image
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Image ByteToImage(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length);
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(ms);
            ms.Close();
            return (Image)obj;
        }
        #endregion

        #region base64转换为image格式
        /// <summary>
        /// base64转换为image格式
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static Image Base64ToImage(string base64String)
        {
            byte[] bitmapData = new byte[base64String.Length];
            // Convert base 64 string to byte[]
            bitmapData = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            var streamBitmap = new MemoryStream(bitmapData);
            Image image = Image.FromStream(streamBitmap, true);
            return image;
        }
        #endregion

        #endregion

        #region 其他格式保存到指定目录

        #region base64格式保存png图片
        /// <summary>
        /// base64格式保存图片
        /// </summary>
        /// <param name="base64">base64 string</param>
        /// <param name="path">图片绝对路径(不带图片名称)</param>
        /// <param name="imageName">图片名称</param>
        public static void Base64ToSave(string base64, string path, string imageName)
        {
            try
            {
                if (!Directory.Exists(path))//判断目录是否存在
                {
                    if (path != null) Directory.CreateDirectory(path);
                }
                string flieSavePath = path + "\\" + imageName;
                var match = Regex.Match(base64, "data:image/png;base64,([\\w\\W]*)$");
                if (match.Success)
                {
                    base64 = match.Groups[1].Value;
                }
                var photoBytes = Convert.FromBase64String(base64);
                File.WriteAllBytes(flieSavePath, photoBytes);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// base64格式保存png图片
        /// </summary>
        /// <param name="base64">base64 string</param>
        /// <param name="path">图片绝对路径(带图片名称)</param>
        public static void Base64ToSaveImage(string base64, string path)
        {
            try
            {
                string filepath = Path.GetDirectoryName(path);
                if (!Directory.Exists(filepath))//判断目录是否存在
                {
                    if (filepath != null) Directory.CreateDirectory(filepath);
                }
                var match = Regex.Match(base64, "data:image/png;base64,([\\w\\W]*)$");
                if (match.Success)
                {
                    base64 = match.Groups[1].Value;
                }
                var photoBytes = Convert.FromBase64String(base64);
                File.WriteAllBytes(path, photoBytes);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// base64格式保存图片
        /// </summary>
        /// <param name="base64">base64格式</param>
        /// <param name="path">相对路径</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static bool Base64ToSaveImage(string base64, string path, string fileName)
        {
            bool success = false;

            //判断上传目录是否存在
            if (Directory.Exists(Directory.GetCurrentDirectory() + path) == false)//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + path);
            }

            //判断本地文件是否存在
            string filePath = Path.Combine(Directory.GetCurrentDirectory() + path,
                                 fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            //文件保存到本地目录
            Image image = Base64ToImage(base64);
            image.Save(filePath);
            success = true;
            return success;
        }
        #endregion

        #region 文件保存到其他路径
        /// <summary>
        /// 文件保存到其他路径
        /// </summary>
        /// <param name="file">文件</param>
        /// <param name="Name">文件名称</param>
        /// <param name="path">文件相对路径</param>
        public static string SaveFileToImage(IFormFile file, string Name, string path)
        {
            string ext = Path.GetExtension(file.FileName);
            Stream s = file.OpenReadStream();//文件流
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(s);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            string Md5res = sb.ToString();
            string temName = Directory.GetCurrentDirectory() + path;
            string fileSaveLocation = temName + Md5res + Name + ext;
            if (!File.Exists(fileSaveLocation))
            {
                Directory.CreateDirectory(temName);
                using (var statem = File.Create(fileSaveLocation))
                {
                    file.CopyTo(statem);
                }
            }
            path = path.Replace("~/", "/");
            string savePath = path + Md5res + Name + ext;
            return savePath;
        }
        #endregion

        #endregion

        #region 检测图片是不是指定格式
        /// <summary>
        /// 检测图片是不是指定格式
        /// </summary>
        /// <param name="base64">base64格式</param>
        /// <param name="Format">图片格式（jpg、png、gif）</param>
        /// <returns></returns>
        public static bool ImagesFormat(string base64, string Format)
        {
            try
            {
                bool flag = false;
                byte[] img = Convert.FromBase64String(base64);
                var jpgStr = string.Empty;
                switch (Format)
                {
                    case "jpg":
                        jpgStr = "{img[0].ToString()}{img[1].ToString()}";
                        flag = jpgStr.Equals("{(int)Imagetype.PNG}");
                        break;
                    case "png":
                        jpgStr = "{img[0].ToString()}{img[1].ToString()}";
                        flag = jpgStr.Equals("{(int)Imagetype.PNG}");
                        break;
                    case "gif":
                        jpgStr = "{img[0].ToString()}{img[1].ToString()}";
                        flag = jpgStr.Equals("{(int)Imagetype.GIF}");
                        break;
                    default:
                        break;
                }
                return flag;
            }
            catch (Exception)
            {

                return false;
            }

        }
        #endregion

        #region 根据绝对路径获取图片的二进制数组
        /// <summary>
        /// 根据绝对路径获取图片的二进制数组
        /// </summary>
        /// <param name="url">远程路径</param>
        /// <returns></returns>
        public static byte[] GetBytesFromUrl(string url)
        {
            byte[] b;
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            WebResponse myResp = myReq.GetResponse();

            Stream stream = myResp.GetResponseStream();
            //int i;
            using (BinaryReader br = new BinaryReader(stream))
            {
                //i = (int)(stream.Length);
                b = br.ReadBytes(500000);
                br.Close();
            }
            myResp.Close();
            return b;

        }
        #endregion

        #region 保存二进制文件到指定目录
        /// <summary>
        /// 保存二进制文件到指定目录
        /// </summary>
        /// <param name="fileName">文件保存路径</param>
        /// <param name="content">二进制内容</param>
        public static void WriteBytesToFile(string fileName, byte[] content)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            BinaryWriter w = new BinaryWriter(fs);
            try
            {
                w.Write(content);
            }
            finally
            {
                fs.Close();
                w.Close();
            }

        }
        #endregion

        #region 同步/异步上传Base64 图像字符串
        /// <summary>
        /// 同步上传Base64 图像字符串
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="base64String">base64</param>
        /// <param name="path">相对路径</param>
        /// <returns></returns>
        public static bool UploadBase64Image(string fileName, string base64String, string path)
        {
            bool success = false;

            string mapPath = Directory.GetCurrentDirectory() + path;
            //判断上传目录是否存在
            if (Directory.Exists(mapPath) == false)//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + path);
            }

            //判断本地文件是否存在
            string filePath = Path.Combine(Directory.GetCurrentDirectory() + path, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            //文件保存到本地目录
            Image image = Base64ToImage(base64String);
            image.Save(filePath);
            success = true;

            return success;
        }
        #region 异步上传Base64 图像字符串
        /// <summary>
        /// 异步上传Base64 图像字符串
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="base64String">base64</param>
        /// <param name="path">相对路径</param>
        /// <returns></returns>
        public static void AsynUploadBase64Image(string fileName, string base64String, string path)
        {
            Thread t1 = new Thread(() =>
            {
                try
                {
                    string mapPath = AppDomain.CurrentDomain.BaseDirectory + path;
                    //判断上传目录是否存在
                    if (Directory.Exists(mapPath) == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(mapPath);
                    }

                    //判断本地文件是否存在
                    string filePath = Path.Combine(mapPath, fileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    //文件保存到本地目录
                    Image image = Base64ToImage(base64String);
                    image.Save(filePath);
                }
                catch
                {

                }
            });
            t1.Start();
        }
        #endregion

        #endregion

        #region 无损压缩图片
        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sFile">原图片地址</param>
        /// <param name="dFile">压缩后保存图片地址</param>
        /// <param name="flag">压缩质量（数字越小压缩率越高）1-100</param>
        /// <param name="size">压缩后图片的最大大小</param>
        /// <param name="sfsc">是否是第一次调用</param>
        /// <returns></returns>
        public static bool CompressImage(string sFile, string dFile, int flag = 90, int size = 300, bool sfsc = true)
        {
            //如果是第一次调用，原始图像的大小小于要压缩的大小，则直接复制文件，并且返回true
            FileInfo firstFileInfo = new FileInfo(sFile);
            if (sfsc == true && firstFileInfo.Length < size * 1024)
            {
                firstFileInfo.CopyTo(dFile);
                return true;
            }
            Image iSource = Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int dHeight = iSource.Height / 2;
            int dWidth = iSource.Width / 2;
            int sW = 0, sH = 0;
            //按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);
            if (tem_size.Width > dHeight || tem_size.Width > dWidth)
            {
                if (tem_size.Width * dHeight > tem_size.Width * dWidth)
                {
                    sW = dWidth;
                    sH = dWidth * tem_size.Height / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = tem_size.Width * dHeight / tem_size.Height;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();

            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;

            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径
                    FileInfo fi = new FileInfo(dFile);
                    if (fi.Length > 1024 * size)
                    {
                        flag = flag - 10;
                        CompressImage(sFile, dFile, flag, size, false);
                    }
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();
            }
        }
        #endregion

        #region 生成图片水印
        /// <summary>
        /// 生成图片水印
        /// </summary>
        /// <param name="originalPath">源图本地路径</param>
        /// <param name="watermarkPath">水印图片本地路径</param>
        /// <param name="targetPath">保存本地路径</param>
        /// <param name="position">位置（1-9）</param>
        /// <param name="opacity">透明度</param>
        /// <param name="quality">质量</param>
        public static void GenerateImageWatermark(string originalPath, string watermarkPath, string targetPath, int position, int opacity, int quality)
        {
            Image originalImage = null;
            Image watermarkImage = null;
            //图片属性
            ImageAttributes attributes = null;
            //画板
            Graphics g = null;
            try
            {

                originalImage = Image.FromFile(originalPath);
                watermarkImage = new Bitmap(watermarkPath);

                if (watermarkImage.Height >= originalImage.Height || watermarkImage.Width >= originalImage.Width)
                {
                    originalImage.Save(targetPath);
                    return;
                }

                if (quality < 0 || quality > 100)
                    quality = 80;

                //水印透明度
                float iii;
                if (opacity > 0 && opacity <= 10)
                    iii = (float)(opacity / 10.0F);
                else
                    iii = 0.5F;

                //水印位置
                int x = 0;
                int y = 0;
                switch (position)
                {
                    case 1://左上角
                        x = (int)(originalImage.Width * (float).01);
                        y = (int)(originalImage.Height * (float).01);
                        break;
                    case 2://上面中间
                        x = (int)(originalImage.Width * (float).50 - watermarkImage.Width / 2);
                        y = (int)(originalImage.Height * (float).01);
                        break;
                    case 3://右上角
                        x = (int)(originalImage.Width * (float).99 - watermarkImage.Width);
                        y = (int)(originalImage.Height * (float).01);
                        break;
                    case 4://右边中间
                        x = (int)(originalImage.Width * (float).01);
                        y = (int)(originalImage.Height * (float).50 - watermarkImage.Height / 2);
                        break;
                    case 5://正中间
                        x = (int)(originalImage.Width * (float).50 - watermarkImage.Width / 2);
                        y = (int)(originalImage.Height * (float).50 - watermarkImage.Height / 2);
                        break;
                    case 6://左边中间
                        x = (int)(originalImage.Width * (float).99 - watermarkImage.Width);
                        y = (int)(originalImage.Height * (float).50 - watermarkImage.Height / 2);
                        break;
                    case 7://左下角
                        x = (int)(originalImage.Width * (float).01);
                        y = (int)(originalImage.Height * (float).99 - watermarkImage.Height);
                        break;
                    case 8://下面中间
                        x = (int)(originalImage.Width * (float).50 - watermarkImage.Width / 2);
                        y = (int)(originalImage.Height * (float).99 - watermarkImage.Height);
                        break;
                    case 9://右下角
                        x = (int)(originalImage.Width * (float).99 - watermarkImage.Width);
                        y = (int)(originalImage.Height * (float).99 - watermarkImage.Height);
                        break;
                }

                //颜色映射表
                ColorMap colorMap = new ColorMap();
                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                ColorMap[] newColorMap = { colorMap };

                //颜色变换矩阵,iii是设置透明度的范围0到1中的单精度类型
                float[][] newColorMatrix ={
                                            new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                            new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                            new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                            new float[] {0.0f,  0.0f,  0.0f,  iii, 0.0f},
                                            new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                           };
                //定义一个 5 x 5 矩阵
                ColorMatrix matrix = new ColorMatrix(newColorMatrix);

                //图片属性
                attributes = new ImageAttributes();
                attributes.SetRemapTable(newColorMap, ColorAdjustType.Bitmap);
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                //画板
                g = Graphics.FromImage(originalImage);
                //绘制水印
                g.DrawImage(watermarkImage, new Rectangle(x, y, watermarkImage.Width, watermarkImage.Height), 0, 0, watermarkImage.Width, watermarkImage.Height, GraphicsUnit.Pixel, attributes);
                //保存图片
                EncoderParameters encoderParams = new EncoderParameters();
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, new long[] { quality });
                if (GetJPEGCodec() != null)
                    originalImage.Save(targetPath, _jpegcodec, encoderParams);
                else
                    originalImage.Save(targetPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (g != null)
                    g.Dispose();
                if (attributes != null)
                    attributes.Dispose();
                if (watermarkImage != null)
                    watermarkImage.Dispose();
                if (originalImage != null)
                    originalImage.Dispose();
            }
        }
        #region 获得当前系统安装的JPEG编码解码器
        /// <summary>
        /// 获得当前系统安装的JPEG编码解码器
        /// </summary>
        /// <returns></returns>
        public static ImageCodecInfo GetJPEGCodec()
        {
            if (_isloadjpegcodec == true)
                return _jpegcodec;

            ImageCodecInfo[] codecsList = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecsList)
            {
                if (codec.MimeType.IndexOf("jpeg") > -1)
                {
                    _jpegcodec = codec;
                    break;
                }

            }
            _isloadjpegcodec = true;
            return _jpegcodec;
        }
        #endregion
        #endregion

        #region 生成文字水印
        /// <summary>
        /// 生成文字水印
        /// </summary>
        /// <param name="originalPath">源图本地路径</param>
        /// <param name="targetPath">保存的本地路径</param>
        /// <param name="text">水印文字</param>
        /// <param name="textSize">文字大小</param>
        /// <param name="textFont">文字字体</param>
        /// <param name="position">位置（1-9）</param>
        /// <param name="quality">质量</param>
        public static void GenerateTextWatermark(string originalPath, string targetPath, string text, int textSize, string textFont, int position, int quality)
        {
            Image originalImage = null;
            //画板
            Graphics g = null;
            try
            {
                originalImage = Image.FromFile(originalPath);
                //画板
                g = Graphics.FromImage(originalImage);
                if (quality < 0 || quality > 100)
                    quality = 80;

                Font font = new Font(textFont, textSize, FontStyle.Regular, GraphicsUnit.Pixel);
                SizeF sizePair = g.MeasureString(text, font);

                float x = 0;
                float y = 0;

                switch (position)
                {
                    case 1://和上面把图片当成水印的位置相同
                        x = originalImage.Width * (float).01;
                        y = originalImage.Height * (float).01;
                        break;
                    case 2:
                        x = originalImage.Width * (float).50 - sizePair.Width / 2;
                        y = originalImage.Height * (float).01;
                        break;
                    case 3:
                        x = originalImage.Width * (float).99 - sizePair.Width;
                        y = originalImage.Height * (float).01;
                        break;
                    case 4:
                        x = originalImage.Width * (float).01;
                        y = originalImage.Height * (float).50 - sizePair.Height / 2;
                        break;
                    case 5:
                        x = originalImage.Width * (float).50 - sizePair.Width / 2;
                        y = originalImage.Height * (float).50 - sizePair.Height / 2;
                        break;
                    case 6:
                        x = originalImage.Width * (float).99 - sizePair.Width;
                        y = originalImage.Height * (float).50 - sizePair.Height / 2;
                        break;
                    case 7:
                        x = originalImage.Width * (float).01;
                        y = originalImage.Height * (float).99 - sizePair.Height;
                        break;
                    case 8:
                        x = originalImage.Width * (float).50 - sizePair.Width / 2;
                        y = originalImage.Height * (float).99 - sizePair.Height;
                        break;
                    case 9:
                        x = originalImage.Width * (float).99 - sizePair.Width;
                        y = originalImage.Height * (float).99 - sizePair.Height;
                        break;
                }

                g.DrawString(text, font, new SolidBrush(Color.White), x + 1, y + 1);
                g.DrawString(text, font, new SolidBrush(Color.Black), x, y);

                //保存图片
                EncoderParameters encoderParams = new EncoderParameters();
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, new long[] { quality });
                if (GetJPEGCodec() != null)
                    originalImage.Save(targetPath, _jpegcodec, encoderParams);
                else
                    originalImage.Save(targetPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (g != null)
                    g.Dispose();
                if (originalImage != null)
                    originalImage.Dispose();
            }
        }
        #endregion

    }
}
