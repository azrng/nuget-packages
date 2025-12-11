using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace CommonCollect
{
    /// <summary>
    /// 印章帮助类
    /// </summary>
    public class SealHelper
    {

        /*
         操作方法

         /// <summary>
        /// 生成公司印章 圆章
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GenerateCompany()
        {
            using var helper = SealHelper.Mechanism();
            helper.DrawCircle();//绘制圆
            helper.DrawStar();//绘制星星
            helper.DrawTitle("智慧城市有限公司");//绘制公司名称
            helper.DrawHorizontal("练习专用");//绘制横向文
            helper.DrawChord("20191016094709");//绘制下弦文
            //helper.Save(Path.Combine(Directory.GetCurrentDirectory(), "公司印章.png"));
            MemoryStream ms = new MemoryStream();
            helper.Save(ms, ImageFormat.Jpeg);
            return File(ms.GetBuffer(), "image/jpg");
            //或
            //Response.Body.WriteAsync(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
            //Response.Body.Close();
        }

        /// <summary>
        /// 生成个人印章  方章
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public void GenerateMy()
        {
            //绘制方形印章
            using (var helper = SealHelper.Personal())
            {
                helper.DrawSquare();
                helper.DrawName("张三");
                helper.Save(Path.Combine(Directory.GetCurrentDirectory(), "张三.png"));
            }
            //绘制姓名带边框
            using (var helper = SealHelper.Personal())
            {
                helper.DrawNameWithBorder("李四");
                helper.Save(Path.Combine(Directory.GetCurrentDirectory(), "李四.png"));
            }
            //绘制姓名
            using (var helper = SealHelper.Personal())
            {
                helper.DrawName("王五");
                helper.Save(Path.Combine(Directory.GetCurrentDirectory(), "王五.png"));
            }
        }





         */

        public static MechanismSealHelper Mechanism()
        {
            return new MechanismSealHelper();
        }

        public static PersonalSealHelper Personal()
        {
            return new PersonalSealHelper();
        }

        public class MechanismSealHelper : IDisposable
        {
            private readonly string _star = "★";
            private readonly int _size = 160;
            private readonly Image _map;
            private readonly Graphics _g;
            private readonly int _defaultWidth;
            private readonly float _defaultStarSize;
            private readonly float _defaultTitleSize;
            private readonly float _defaultHorizontalSize;
            private readonly float _defaultChordSize;

            public Color Color { get; set; } = Color.Red;
            public string DefaultFontName { get; set; } = "SimSun";

            public MechanismSealHelper()
            {
                _map = new Bitmap(_size, _size);
                _g = Graphics.FromImage(_map);//实例化Graphics类
                _g.SmoothingMode = SmoothingMode.AntiAlias;  //Nuget System.Drawing.Drawing2D;
                _g.Clear(Color.Transparent);

                _defaultWidth = _size / 40;
                _defaultStarSize = _size / 5;
                _defaultTitleSize = (float)Math.Sqrt(_size);
                _defaultHorizontalSize = (float)Math.Sqrt(_size);
                _defaultChordSize = _size / 20;
            }

            /// <summary>
            /// 绘制圆
            /// </summary>
            public void DrawCircle()
            {
                DrawCircle(_defaultWidth);
            }

            /// <summary>
            /// 绘制圆
            /// </summary>
            /// <param name="width">画笔粗细</param>
            public void DrawCircle(int width)
            {
                Rectangle rect = new Rectangle(width, width, _size - width * 2, _size - width * 2);//设置圆的绘制区域
                Pen pen = new Pen(Color, width);  //设置画笔（颜色和粗细）
                _g.DrawEllipse(pen, rect);  //绘制圆
            }

            /// <summary>
            /// 绘制星星
            /// </summary>
            public void DrawStar()
            {
                DrawStar(_defaultStarSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制星星
            /// </summary>
            /// <param name="emSize"></param>
            public void DrawStar(float emSize)
            {
                DrawStar(emSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制星星
            /// </summary>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawStar(float emSize, int width)
            {
                Font starFont = new Font(DefaultFontName, emSize, FontStyle.Bold);//设置星号的字体样式
                var starSize = _g.MeasureString(_star, starFont);//对指定字符串进行测量
                                                                 //要指定的位置绘制星号
                PointF starXy = new PointF(_size / 2 - starSize.Width / 2, _size / 2 - starSize.Height / 2);
                _g.DrawString(_star, starFont, new SolidBrush(Color), starXy);
            }

            /// <summary>
            /// 绘制主题
            /// </summary>
            /// <param name="title">主题（公司名称）</param>
            /// <param name="startAngle">开始角度</param>
            public void DrawTitle(string title, float startAngle = 160)
            {
                DrawTitle(title, startAngle, _defaultTitleSize);
            }

            /// <summary>
            /// 绘制主题
            /// </summary>
            /// <param name="title">主题（公司名称）</param>
            /// <param name="startAngle">开始角度,必须是左半边，推荐（135-270）</param>
            /// <param name="emSize">字体大小</param>
            public void DrawTitle(string title, float startAngle, float emSize)
            {
                DrawTitle(title, startAngle, emSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制主题
            /// </summary>
            /// <param name="title">主题（公司名称）</param>
            /// <param name="startAngle">开始角度,必须是左半边，推荐（135-270）</param>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawTitle(string title, float startAngle, float emSize, int width)
            {
                Font font = new Font(DefaultFontName, emSize, FontStyle.Bold);
                DrawTitle(title, startAngle, font, width);
            }

            /// <summary>
            /// 绘制主题
            /// </summary>
            /// <param name="title">主题（公司名称）</param>
            /// <param name="startAngle">开始角度,必须是左半边，推荐（135-270）</param>
            /// <param name="font">字体</param>
            /// <param name="width">画笔粗细</param>
            public void DrawTitle(string title, float startAngle, Font font, int width)
            {
                if (string.IsNullOrEmpty(title))
                {
                    return;
                }
                if (Math.Cos(startAngle / 180 * Math.PI) > 0)
                {
                    throw new ArgumentException($"初始角度错误：{startAngle}(建议135-270)", nameof(startAngle));
                }

                startAngle = startAngle % 360;//起始角度

                var length = title.Length;
                float changeAngle = (270 - startAngle) * 2 / length;//每个字所占的角度，也就是旋转角度
                var circleWidth = _size / 2 - width * 3;//字体圆形的长度
                var fontSize = _g.MeasureString(title, font);//测量一下字体
                var per = fontSize.Width / length;//每个字体的长度

                //在指定的角度绘制字符
                void action(string t, float a)
                {
                    var angleXy = a / 180 * Math.PI;
                    var x = _size / 2 + Math.Cos(angleXy) * circleWidth;
                    var y = _size / 2 + Math.Sin(angleXy) * circleWidth;

                    DrawChar(t, (float)x, (float)y, a + 90, font, width);
                }

                //起始绘制角度=起始角度+旋转角度/2-字体所占角度的一半
                var angle = startAngle + changeAngle / 2 - (float)(Math.Asin(per / 2 / circleWidth) / Math.PI * 180);//起始绘制角度
                for (var i = 0; i < length; i++)
                {
                    action(title[i].ToString(), angle);
                    angle += changeAngle;
                }
            }

            /// <summary>
            /// 绘制横向文
            /// </summary>
            /// <param name="text">横向文</param>
            public void DrawHorizontal(string text)
            {
                DrawHorizontal(text, _defaultHorizontalSize);
            }

            /// <summary>
            /// 绘制横向文
            /// </summary>
            /// <param name="text">横向文</param>
            /// <param name="emSize">字体大小</param>
            public void DrawHorizontal(string text, float emSize)
            {
                Font font = new Font(DefaultFontName, emSize, FontStyle.Bold);//定义字体的字体样式
                DrawHorizontal(text, font);
            }

            /// <summary>
            /// 绘制横向文
            /// </summary>
            /// <param name="text">横向文</param>
            /// <param name="font">字体</param>
            public void DrawHorizontal(string text, Font font)
            {
                SizeF textSize = _g.MeasureString(text, font);//对指定字符串进行测量
                while (textSize.Width > _size * 2 / 3)
                {
                    DrawHorizontal(text, new Font(font.Name, font.Size - 1, font.Style));
                    return;
                }
                //要指定的位置绘制中间文字
                PointF point = new PointF(_size / 2 - textSize.Width / 2, _size * 2 / 3);

                _g.ResetTransform();
                _g.DrawString(text, font, new SolidBrush(Color), point);
            }

            /// <summary>
            /// 绘制下弦文
            /// </summary>
            /// <param name="text">下弦文</param>
            /// <param name="startAngle">开始角度,必须是左下半边，推荐（90-180）</param>
            public void DrawChord(string text, float startAngle = 135)
            {
                DrawChord(text, startAngle, _defaultChordSize);
            }

            /// <summary>
            /// 绘制下弦文
            /// </summary>
            /// <param name="text">下弦文</param>
            /// <param name="startAngle">开始角度,必须是左下半边，推荐（90-180）</param>
            /// <param name="emSize">字体大小</param>
            public void DrawChord(string text, float startAngle, float emSize)
            {
                DrawChord(text, startAngle, emSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制下弦文
            /// </summary>
            /// <param name="text">下弦文</param>
            /// <param name="startAngle">开始角度,必须是左下半边，推荐（90-180）</param>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawChord(string text, float startAngle, float emSize, int width)
            {
                Font font = new Font(DefaultFontName, emSize, FontStyle.Bold);//定义字体的字体样式
                DrawChord(text, startAngle, font, width);
            }

            /// <summary>
            /// 绘制下弦文
            /// </summary>
            /// <param name="text">下弦文</param>
            /// <param name="startAngle">开始角度,必须是左下半边，推荐（90-180）</param>
            /// <param name="font">字体</param>
            /// <param name="width">画笔粗细</param>
            public void DrawChord(string text, float startAngle, Font font, int width)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }
                if (Math.Cos(startAngle / 180 * Math.PI) > 0)
                {
                    throw new ArgumentException($"初始角度错误：{startAngle}(建议90-135)", nameof(startAngle));
                }

                startAngle = startAngle % 360;//起始角度

                var length = text.Length;
                float changeAngle = (startAngle - 90) * 2 / length;//每个字所占的角度，也就是旋转角度
                var fontSize = _g.MeasureString(text, font);//测量一下字体
                var per = fontSize.Width / length;//每个字体的长度
                var circleWidth = _size / 2 - width * 2 - fontSize.Height;//字体圆形的长度

                //在指定的角度绘制字符
                void action(string t, float a)
                {
                    var angleXy = a / 180 * Math.PI;
                    var x = _size / 2 + Math.Cos(angleXy) * circleWidth;
                    var y = _size / 2 + Math.Sin(angleXy) * circleWidth;

                    DrawChar(t, (float)x, (float)y, a - 90, font, width);
                }

                //起始绘制角度=起始角度-旋转角度/2+字体所占角度的一半
                var angle = startAngle - changeAngle / 2 + (float)(Math.Asin(per / 2 / circleWidth) / Math.PI * 180);//起始绘制角度
                for (var i = 0; i < length; i++)
                {
                    action(text[i].ToString(), angle);
                    angle -= changeAngle;
                }
            }

            /// <summary>
            /// 绘制单个字符
            /// </summary>
            /// <param name="char">字符</param>
            /// <param name="x">距离画布左边的距离</param>
            /// <param name="y">距离画布上边的距离</param>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawChar(string @char, float x, float y, float emSize)
            {
                DrawChar(@char, x, y, 0, emSize);
            }

            /// <summary>
            /// 绘制单个字符
            /// </summary>
            /// <param name="char">字符</param>
            /// <param name="x">距离画布左边的距离</param>
            /// <param name="y">距离画布上边的距离</param>
            /// <param name="angle">选中角度，0度为右方，顺时针增加</param>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawChar(string @char, float x, float y, float angle, float emSize)
            {
                DrawChar(@char, x, y, angle, emSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制单个字符
            /// </summary>
            /// <param name="char">字符</param>
            /// <param name="x">距离画布左边的距离</param>
            /// <param name="y">距离画布上边的距离</param>
            /// <param name="angle">选中角度，0度为右方，顺时针增加</param>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawChar(string @char, float x, float y, float angle, float emSize, int width)
            {
                Font font = new Font(DefaultFontName, emSize, FontStyle.Bold);
                DrawChar(@char, x, y, angle, font, width);
            }

            /// <summary>
            /// 绘制单个字符
            /// </summary>
            /// <param name="char">字符</param>
            /// <param name="x">距离画布左边的距离</param>
            /// <param name="y">距离画布上边的距离</param>
            /// <param name="angle">选中角度，0度为右方，顺时针增加</param>
            /// <param name="fontName">字体</param>
            /// <param name="width">画笔粗细</param>
            public void DrawChar(string @char, float x, float y, float angle, Font font, int width)
            {
                if (string.IsNullOrEmpty(@char) || @char.Length != 1)
                {
                    throw new ArgumentException("only one char is supported", nameof(@char));
                }

                _g.ResetTransform();//重置
                _g.TranslateTransform(x, y);//调整偏移
                _g.RotateTransform(angle);//旋转角度
                _g.DrawString(@char, font, new SolidBrush(Color), 0, 0);//绘制，因为使用了偏移，所以这里的坐标是相对偏移的，所以是0
            }

            /// <summary>
            /// 保存图片
            /// </summary>
            /// <param name="fileName"></param>
            public void Save(string fileName)
            {
                _map.Save(fileName);
            }

            /// <summary>
            /// 保存图片
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="format"></param>
            public void Save(Stream stream, ImageFormat format)
            {
                _map.Save(stream, format);
            }



            public void Dispose()
            {
                try
                {
                    if (_map != null)
                    {
                        _map.Dispose();
                    }
                    if (_g != null)
                    {
                        _g.Dispose();
                    }
                }
                catch { }
            }
        }

        public class PersonalSealHelper : IDisposable
        {
            private readonly int _size = 160;
            private readonly Image _map;
            private readonly Graphics _g;
            private readonly int _defaultWidth;
            private readonly float _defaultSquareSize;
            private readonly float _defaultNameSize;
            private bool _isSquare = false;

            public Color Color { get; set; } = Color.Red;
            public string DefaultFontName { get; set; } = "SimSun";

            public PersonalSealHelper()
            {
                _map = new Bitmap(_size, _size);
                _g = Graphics.FromImage(_map);//实例化Graphics类
                _g.SmoothingMode = SmoothingMode.AntiAlias;  //System.Drawing.Drawing2D;
                _g.Clear(Color.Transparent);

                _defaultWidth = _size / 40;
                _defaultSquareSize = _size / 4;
                _defaultNameSize = _size / 4;
            }

            /// <summary>
            /// 绘制方形之印
            /// </summary>
            public void DrawSquare()
            {
                DrawSquare(_defaultSquareSize);
            }

            /// <summary>
            /// 绘制方形之印
            /// </summary>
            /// <param name="emSize">字体大小</param>
            public void DrawSquare(float emSize)
            {
                DrawSquare(emSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制方形之印
            /// </summary>
            /// <param name="font">字体</param>
            public void DrawSquare(Font font)
            {
                DrawSquare(font, _defaultWidth);
            }

            /// <summary>
            /// 绘制方形之印
            /// </summary>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawSquare(float emSize, int width)
            {
                Font font = new Font(DefaultFontName, emSize, FontStyle.Bold);//设置之印的字体样式
                DrawSquare(font, width);
            }

            /// <summary>
            /// 绘制方形之印
            /// </summary>
            /// <param name="font">字体</param>
            /// <param name="width">画笔粗细</param>
            public void DrawSquare(Font font, int width)
            {
                _isSquare = true;

                var pen = new Pen(Color, width);//设置画笔的颜色
                Rectangle rect = new Rectangle(width, width, _size - width * 2, _size - width * 2);//设置绘制区域
                _g.DrawRectangle(pen, rect);

                var textSize = _g.MeasureString("之印", font);//对指定字符串进行测量
                var left = (_size / 2 - width * 2 - textSize.Width / 2) / 2;
                var perHeght = (_size - width * 4 - textSize.Height * 2) / 3;

                PointF point1 = new PointF(left + width * 2, perHeght + width * 2);
                _g.DrawString("之", font, pen.Brush, point1);

                PointF point2 = new PointF(left + width * 2, perHeght * 2 + width * 2 + textSize.Height);
                _g.DrawString("印", font, pen.Brush, point2);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            public void DrawNameWithBorder(string name)
            {
                DrawNameWithBorder(name, _defaultNameSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            /// <param name="font">字体</param>
            public void DrawNameWithBorder(string name, Font font)
            {
                DrawNameWithBorder(name, font, _defaultWidth);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            /// <param name="emSize">字体大小</param>
            public void DrawNameWithBorder(string name, float emSize)
            {
                DrawNameWithBorder(name, emSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawNameWithBorder(string name, float emSize, int width)
            {
                Font font = new Font(DefaultFontName, emSize, FontStyle.Bold);//设置字体样式
                DrawNameWithBorder(name, font, width);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            /// <param name="font">字体</param>
            /// <param name="width">画笔粗细</param>
            public void DrawNameWithBorder(string name, Font font, int width)
            {
                var nameSize = _g.MeasureString(name, font);//对指定字符串进行测量
                while (nameSize.Width > _size - width * 6)
                {
                    DrawNameWithBorder(name, new Font(font.Name, font.Size - 1, font.Style), width);
                    return;
                }
                var left = (int)(_size - nameSize.Width - width * 4) / 2;
                var height = (int)(_size - nameSize.Height - width * 4) / 2;

                var pen = new Pen(Color, width);//设置画笔的颜色
                Rectangle rect = new Rectangle(width, height, _size - width * 2, _size - height * 2);//设置绘制区域
                _g.DrawRectangle(pen, rect);

                PointF point = new PointF(width + width * 2, height + width * 2);
                _g.DrawString(name, font, pen.Brush, point);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            public void DrawName(string name)
            {
                DrawName(name, _defaultNameSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            /// <param name="font">字体</param>
            public void DrawName(string name, Font font)
            {
                DrawName(name, font, _defaultWidth);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            /// <param name="emSize">字体大小</param>
            public void DrawName(string name, float emSize)
            {
                DrawName(name, emSize, _defaultWidth);
            }

            /// <summary>
            /// 绘制带边框的姓名
            /// </summary>
            /// <param name="name">姓名</param>
            /// <param name="emSize">字体大小</param>
            /// <param name="width">画笔粗细</param>
            public void DrawName(string name, float emSize, int width)
            {
                Font font = new Font(DefaultFontName, emSize, FontStyle.Bold);//设置字体样式
                DrawName(name, font, width);
            }

            /// <summary>
            /// 绘制姓名
            /// </summary>
            /// <param name="name">姓名</param>
            /// <param name="font">字体</param>
            /// <param name="width">画笔粗细</param>
            public void DrawName(string name, Font font, int width)
            {
                var nameSize = _g.MeasureString(name, font);//对指定字符串进行测量
                while (nameSize.Width > _size - width * 6)
                {
                    DrawName(name, new Font(font.Name, font.Size - 1, font.Style), width);
                    return;
                }
                if (_isSquare)
                {
                    int length = name.Length;//获取字符串的长度
                    var left = (_size / 2 - width - nameSize.Width / length) / 2;
                    var height = (_size - width * 4 - nameSize.Height * length) / (length + 1);
                    if (left <= 0 || height <= 0)
                    {
                        return;
                    }

                    for (var i = 0; i < length; i++)
                    {
                        PointF point = new PointF(width + _size / 2, height * (i + 1) + width * 2 + nameSize.Height * i);
                        _g.DrawString(name[i].ToString(), font, new SolidBrush(Color), point);
                    }
                }
                else
                {
                    var left = (_size - nameSize.Width) / 2;
                    var height = (_size - nameSize.Height) / 2;
                    PointF point = new PointF(width, height);
                    _g.DrawString(name, font, new SolidBrush(Color), point);
                }
            }

            /// <summary>
            /// 保存图片
            /// </summary>
            /// <param name="fileName"></param>
            public void Save(string fileName)
            {
                _map.Save(fileName);
            }

            /// <summary>
            /// 保存图片
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="format"></param>
            public void Save(Stream stream, ImageFormat format)
            {
                _map.Save(stream, format);
            }

            public void Dispose()
            {
                try
                {
                    if (_map != null)
                    {
                        _map.Dispose();
                    }
                    if (_g != null)
                    {
                        _g.Dispose();
                    }
                }
                catch { }
            }
        }
    }
}
