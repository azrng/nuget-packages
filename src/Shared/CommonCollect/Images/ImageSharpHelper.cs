using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using SkiaSharp.QrCode;
using System;

namespace CommonCollect
{
    /// <summary>
    /// 跨平台图片帮助类
    /// </summary>
    public static class ImageSharpHelper
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="text">二维码内容</param>
        /// <returns></returns>
        public static byte[] GetQrCode(string text)
        {
            using QRCodeGenerator generator = new();
            using var qr = generator.CreateQrCode(text, ECCLevel.L);
            SKImageInfo info = new(500, 500);

            using var surface = SKSurface.Create(info);
            using var canvas = surface.Canvas;
            canvas.Render(qr, info.Width, info.Height, SKColors.White, SKColors.Black);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// 从图片截取部分区域
        /// </summary>
        /// <param name="fromImagePath">源图路径</param>
        /// <param name="offsetX">距上</param>
        /// <param name="offsetY">距左</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public static byte[] Screenshot(string fromImagePath, int offsetX, int offsetY, int width, int height)
        {
            using var original = SKBitmap.Decode(fromImagePath);
            using SKBitmap bitmap = new(width, height);
            using SKCanvas canvas = new(bitmap);
            SKRect sourceRect = new(offsetX, offsetY, offsetX + width, offsetY + height);
            SKRect destRect = new(0, 0, width, height);

            canvas.DrawBitmap(original, sourceRect, destRect);

            using var img = SKImage.FromBitmap(bitmap);
            using SKData p = img.Encode(SKEncodedImageFormat.Png, 100);
            return p.ToArray();
        }


        /// <summary>
        /// 获取图像数字验证码
        /// </summary>
        /// <param name="text">验证码内容，如4为数字</param>
        /// <returns></returns>
        public static byte[] GetVerifyCode(string text)
        {
            int width = 128;
            int height = 45;

            Random random = new();

            //创建bitmap位图
            using SKBitmap image = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            //创建画笔
            using SKCanvas canvas = new(image);
            //填充背景颜色为白色
            canvas.DrawColor(SKColors.White);

            //画图片的背景噪音线
            for (int i = 0; i < (width * height * 0.015); i++)
            {
                using SKPaint drawStyle = new();
                drawStyle.Color = new(Convert.ToUInt32(random.Next(Int32.MaxValue)));

                canvas.DrawLine(random.Next(0, width), random.Next(0, height), random.Next(0, width),
                    random.Next(0, height), drawStyle);
            }

            //将文字写到画布上
            using (SKPaint drawStyle = new())
            {
                drawStyle.Color = SKColors.Red;
                drawStyle.TextSize = height;
                drawStyle.StrokeWidth = 1;

                float emHeight = height - (float)height * (float)0.14;
                float emWidth = ((float)width / text.Length) - ((float)width * (float)0.13);

                canvas.DrawText(text, emWidth, emHeight, drawStyle);
            }

            //画图片的前景噪音点
            for (int i = 0; i < (width * height * 0.6); i++)
            {
                image.SetPixel(random.Next(0, width), random.Next(0, height),
                    new SKColor(Convert.ToUInt32(random.Next(Int32.MaxValue))));
            }

            using var img = SKImage.FromBitmap(image);
            using SKData p = img.Encode(SKEncodedImageFormat.Png, 100);
            return p.ToArray();
        }

        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="picOneBase64Str">图片base64</param>
        /// <param name="picTwoBase64Str">图片base64</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public static string MergeImage(string picOneBase64Str, string picTwoBase64Str, int x = 0, int y = 0)
        {
            if (string.IsNullOrEmpty(picOneBase64Str))
            {
                throw new ArgumentException("请传入第一张图片base64");
            }

            if (string.IsNullOrEmpty(picTwoBase64Str))
            {
                throw new ArgumentException("请传入第二张图片base64");
            }

            if (x < 0 || y < 0)
            {
                throw new ArgumentException("坐标不能传入负数");
            }

            try
            {
                byte[] array = Convert.FromBase64String(picOneBase64Str);
                byte[] array2 = Convert.FromBase64String(picTwoBase64Str);
                Image obj = Image.Load(array, out IImageFormat val);
                Image outputImg = Image.Load(array2);
                if (obj.Height - (outputImg.Height + y) <= 0)
                {
                    throw new ArgumentException("Y坐标高度超限");
                }

                if (obj.Width - (outputImg.Width + x) <= 0)
                {
                    throw new ArgumentException("X坐标宽度超限");
                }

                ProcessingExtensions.Mutate(obj, (Action<IImageProcessingContext>)delegate(IImageProcessingContext img)
                {
                    //IL_0013: Unknown result type (might be due to invalid IL or missing references)
                    DrawImageExtensions.DrawImage(img, (Image)(object)outputImg, new Point(x, y), 1f);
                });
                return ImageExtensions.ToBase64String(obj, val);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("合并图片错误，错误原因:" + ex.Message);
            }
        }

        /// <summary>
        /// 绘制带字图片
        /// </summary>
        /// <param name="picBase64Str"></param>
        /// <param name="fontFunc"></param>
        /// <param name="typeface"></param>
        /// <param name="textInfos"></param>
        /// <returns></returns>
        public static string DrawText(string picBase64Str, Func<FontFamily, Font> fontFunc,
            string typeface = "SIMHEI.TTF", params (string, Color, int, int)[] textInfos)
        {
            //IL_0027: Unknown result type (might be due to invalid IL or missing references)
            //IL_007e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0083: Unknown result type (might be due to invalid IL or missing references)
            if (string.IsNullOrEmpty(picBase64Str))
            {
                throw new ArgumentException("请传入图片信息");
            }

            Image val2 = Image.Load(Convert.FromBase64String(picBase64Str), out IImageFormat val);
            FontFamily arg = new FontCollection().Install("\\Application.Extension.Infrastructure\\Fonts\\" + typeface);
            Font funcResult = fontFunc(arg);
            foreach ((string, Color, int, int) tuple in textInfos)
            {
                var (text, color, x, y) = tuple;
                ProcessingExtensions.Mutate(val2, (IImageProcessingContext img) =>
                {
                    //IL_0013: Unknown result type (might be due to invalid IL or missing references)
                    //IL_0026: Unknown result type (might be due to invalid IL or missing references)
                    DrawTextExtensions.DrawText(img, text, funcResult, color, new PointF(x, y));
                });
            }

            return ImageExtensions.ToBase64String(val2, val);
        }
    }
}