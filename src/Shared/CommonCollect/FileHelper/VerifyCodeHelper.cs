//using System;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;
//using System.IO;

//namespace CommonCollect
//{
//    public class VerifyCodeHelper
//    {
//        #region 单例模式

//        //创建私有化静态obj锁
//        private static readonly object _objLock = new object();

//        //创建私有静态字段，接收类的实例化对象
//        private static VerifyCodeHelper _verifyCodeHelper = null;

//        //构造函数私有化
//        private VerifyCodeHelper() { }

//        //创建单利对象资源并返回
//        public static VerifyCodeHelper GetSingleObj()
//        {
//            if (_verifyCodeHelper == null)
//            {
//                lock (_objLock)
//                {
//                    if (_verifyCodeHelper == null)
//                        _verifyCodeHelper = new VerifyCodeHelper();
//                }
//            }
//            return _verifyCodeHelper;
//        }

//        #endregion

//        #region 生产验证码

//        public enum VerifyCodeType { NumberVerifyCode, AbcVerifyCode, MixVerifyCode };

//        /// <summary>
//        /// 1.数字验证码
//        /// </summary>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        private string CreateNumberVerifyCode(int length)
//        {
//            int[] randMembers = new int[length];
//            int[] validateNums = new int[length];
//            string validateNumberStr = "";
//            //生成起始序列值
//            int seekSeek = unchecked((int)DateTime.Now.Ticks);
//            Random seekRand = new Random(seekSeek);
//            int beginSeek = seekRand.Next(0, Int32.MaxValue - length * 10000);
//            int[] seeks = new int[length];
//            for (int i = 0; i < length; i++)
//            {
//                beginSeek += 10000;
//                seeks[i] = beginSeek;
//            }
//            //生成随机数字
//            for (int i = 0; i < length; i++)
//            {
//                Random rand = new Random(seeks[i]);
//                int pownum = 1 * (int)Math.Pow(10, length);
//                randMembers[i] = rand.Next(pownum, Int32.MaxValue);
//            }
//            //抽取随机数字
//            for (int i = 0; i < length; i++)
//            {
//                string numStr = randMembers[i].ToString();
//                int numLength = numStr.Length;
//                Random rand = new Random();
//                int numPosition = rand.Next(0, numLength - 1);
//                validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
//            }
//            //生成验证码
//            for (int i = 0; i < length; i++)
//            {
//                validateNumberStr += validateNums[i].ToString();
//            }
//            return validateNumberStr;
//        }

//        /// <summary>
//        /// 2.字母验证码
//        /// </summary>
//        /// <param name="length">字符长度</param>
//        /// <returns>验证码字符</returns>
//        private string CreateAbcVerifyCode(int length)
//        {
//            char[] verification = new char[length];
//            char[] dictionary = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
//                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
//            };
//            Random random = new Random();
//            for (int i = 0; i < length; i++)
//            {
//                verification[i] = dictionary[random.Next(dictionary.Length - 1)];
//            }
//            return new string(verification);
//        }

//        /// <summary>
//        /// 3.混合验证码
//        /// </summary>
//        /// <param name="length">字符长度</param>
//        /// <returns>验证码字符</returns>
//        private string CreateMixVerifyCode(int length)
//        {
//            char[] verification = new char[length];
//            char[] dictionary = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
//                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
//                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
//            };
//            Random random = new Random();
//            for (int i = 0; i < length; i++)
//            {
//                verification[i] = dictionary[random.Next(dictionary.Length - 1)];
//            }
//            return new string(verification);
//        }

//        /// <summary>
//        /// 产生验证码（随机产生4-6位）
//        /// </summary>
//        /// <param name="type">验证码类型：数字，字符，符合</param>
//        /// <returns></returns>
//        public string CreateVerifyCode(VerifyCodeType type)
//        {
//            string verifyCode = string.Empty;
//            Random random = new Random();
//            int length = 5;//random.Next(4, 6);
//            switch (type)
//            {
//                case VerifyCodeType.NumberVerifyCode:
//                    verifyCode = GetSingleObj().CreateNumberVerifyCode(length);
//                    break;

//                case VerifyCodeType.AbcVerifyCode:
//                    verifyCode = GetSingleObj().CreateAbcVerifyCode(length);
//                    break;

//                case VerifyCodeType.MixVerifyCode:
//                    verifyCode = GetSingleObj().CreateMixVerifyCode(length);
//                    break;
//            }
//            return verifyCode;
//        }

//        #endregion

//        #region 验证码图片

//        /// <summary>
//        /// 验证码图片 => Bitmap
//        /// </summary>
//        /// <param name="verifyCode">验证码</param>
//        /// <param name="width">宽</param>
//        /// <param name="height">高</param>
//        /// <returns>Bitmap</returns>
//        public Bitmap CreateBitmapByImgVerifyCode(string verifyCode, int width, int height)
//        {
//            Font font = new Font("Arial", 14, (FontStyle.Bold | FontStyle.Italic));
//            Brush brush;
//            Bitmap bitmap = new Bitmap(width, height);
//            Graphics g = Graphics.FromImage(bitmap);
//            SizeF totalSizeF = g.MeasureString(verifyCode, font);
//            SizeF curCharSizeF;
//            PointF startPointF = new PointF(0, (height - totalSizeF.Height) / 2);
//            Random random = new Random(); //随机数产生器
//            g.Clear(Color.White); //清空图片背景色
//            for (int i = 0; i < verifyCode.Length; i++)
//            {
//                brush = new LinearGradientBrush(new Point(0, 0), new Point(1, 1), Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)), Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)));
//                g.DrawString(verifyCode[i].ToString(), font, brush, startPointF);
//                curCharSizeF = g.MeasureString(verifyCode[i].ToString(), font);
//                startPointF.X += curCharSizeF.Width;
//            }

//            //画图片的干扰线
//            for (int i = 0; i < 10; i++)
//            {
//                int x1 = random.Next(bitmap.Width);
//                int x2 = random.Next(bitmap.Width);
//                int y1 = random.Next(bitmap.Height);
//                int y2 = random.Next(bitmap.Height);
//                g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
//            }

//            //画图片的前景干扰点
//            for (int i = 0; i < 100; i++)
//            {
//                int x = random.Next(bitmap.Width);
//                int y = random.Next(bitmap.Height);
//                bitmap.SetPixel(x, y, Color.FromArgb(random.Next()));
//            }

//            g.DrawRectangle(new Pen(Color.Silver), 0, 0, bitmap.Width - 1, bitmap.Height - 1); //画图片的边框线
//            g.Dispose();
//            return bitmap;
//        }

//        /// <summary>
//        /// 验证码图片 => byte[]
//        /// </summary>
//        /// <param name="verifyCode">验证码</param>
//        /// <param name="width">宽</param>
//        /// <param name="height">高</param>
//        /// <returns>byte[]</returns>
//        public byte[] CreateByteByImgVerifyCode(string verifyCode, int width, int height)
//        {
//            Font font = new Font("Arial", 14, (FontStyle.Bold | FontStyle.Italic));
//            Brush brush;
//            Bitmap bitmap = new Bitmap(width, height);
//            Graphics g = Graphics.FromImage(bitmap);
//            SizeF totalSizeF = g.MeasureString(verifyCode, font);
//            SizeF curCharSizeF;
//            PointF startPointF = new PointF(0, (height - totalSizeF.Height) / 2);
//            Random random = new Random(); //随机数产生器
//            g.Clear(Color.White); //清空图片背景色
//            for (int i = 0; i < verifyCode.Length; i++)
//            {
//                brush = new LinearGradientBrush(new Point(0, 0), new Point(1, 1), Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)), Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)));
//                g.DrawString(verifyCode[i].ToString(), font, brush, startPointF);
//                curCharSizeF = g.MeasureString(verifyCode[i].ToString(), font);
//                startPointF.X += curCharSizeF.Width;
//            }

//            //画图片的干扰线
//            for (int i = 0; i < 10; i++)
//            {
//                int x1 = random.Next(bitmap.Width);
//                int x2 = random.Next(bitmap.Width);
//                int y1 = random.Next(bitmap.Height);
//                int y2 = random.Next(bitmap.Height);
//                g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
//            }

//            //画图片的前景干扰点
//            for (int i = 0; i < 100; i++)
//            {
//                int x = random.Next(bitmap.Width);
//                int y = random.Next(bitmap.Height);
//                bitmap.SetPixel(x, y, Color.FromArgb(random.Next()));
//            }

//            g.DrawRectangle(new Pen(Color.Silver), 0, 0, bitmap.Width - 1, bitmap.Height - 1); //画图片的边框线
//            g.Dispose();

//            //保存图片数据
//            MemoryStream stream = new MemoryStream();
//            bitmap.Save(stream, ImageFormat.Jpeg);
//            //输出图片流
//            return stream.ToArray();
//        }

//        #endregion

//        /// <summary>
//        /// 彩色验证码
//        /// </summary>
//        /// <param name="code"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public byte[] CreateImage(out string code, int length = 4)
//        {
//            code = CommonHelper.RndNum(length);//生成随机数
//            Random random = new Random();
//            //颜色集合
//            Color[] color = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
//            //字体集合
//            string[] fonts = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋体" };
//            //定义图像的大小，生成图像的实例
//            Bitmap Img = new Bitmap(code.Length * 17, 32);
//            Graphics graphics = Graphics.FromImage(Img);
//            graphics.Clear(Color.White);//背景设为白色

//            //在随机位置画背景点

//            for (int i = 0; i < 100; i++)
//            {
//                int x = random.Next(Img.Width);
//                int y = random.Next(Img.Height);
//                graphics.DrawRectangle(new Pen(Color.LightGray, 0), x, y, 1, 1);
//            }

//            //验证码绘制在graphics中
//            for (int i = 0; i < code.Length; i++)
//            {
//                int colorIndex = random.Next(7);//随机颜色索引值
//                int fontIndex = random.Next(4);//随机字体索引值
//                int fontSize = random.Next(13, 18);
//                Font font = new Font(fonts[fontIndex], fontSize, FontStyle.Bold);//字体
//                Brush brush = new SolidBrush(color[colorIndex]);//颜色
//                int y = 4;
//                if ((i + 1) % 2 == 0)//控制验证码不在同一高度
//                {
//                    y = 2;
//                }
//                graphics.DrawString(code.Substring(i, 1), font, brush, 3 + (i * 13), y);//绘制一个验证字符
//            }
//            MemoryStream ms = new MemoryStream();
//            Img.Save(ms, ImageFormat.Png);//将此图像以Png图像文件的格式保存到流中
//            graphics.Dispose();
//            Img.Dispose();
//            ms.Close();
//            ms.Dispose();

//            return ms.GetBuffer();
//        }
//    }
//}