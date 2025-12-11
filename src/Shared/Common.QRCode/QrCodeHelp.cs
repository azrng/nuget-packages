using System;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Common.QRCode
{
    public class QrCodeHelp : IQrCodeHelp
    {
        ///<inheritdoc cref="IQrCodeHelp.CreateQrCode"/>
        public byte[] CreateQrCode(string content, int width = 100, int height = 100)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE
            };
            var options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = width,
                Height = height,
                Margin = 1
            };
            //设置内容编码
            //设置二维码的宽度和高度
            //设置二维码的边距,单位不是固定像素
            writer.Options = options;
            var pixdata = writer.Write(content);
            var bitmap = PixToBitmap(pixdata.Pixels, width, height);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            return ms.GetBuffer();
        }

        ///<inheritdoc cref="IQrCodeHelp.CreateBarCode"/>
        public byte[] CreateBarCode(string content, int width = 100, int height = 100)
        {
            var options = new EncodingOptions()
            {
                Width = width,
                Height = height,
                Margin = 2
            };
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = options
            };

            var pixdata = writer.Write(content);
            var bitmap = PixToBitmap(pixdata.Pixels, width, height);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            return ms.GetBuffer();
        }

        /// <summary>
        /// 将一个字节数组转换为位图
        /// </summary>
        /// <param name="pixValue">显示字节数组</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns>位图</returns>
        private static Bitmap PixToBitmap(byte[] pixValue, int width, int height)
        {
            //// 申请目标位图的变量，并将其内存区域锁定
            var mCurrBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var mRect = new Rectangle(0, 0, width, height);
            var mBitmapData = mCurrBitmap.LockBits(mRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            var iptr = mBitmapData.Scan0;  // 获取bmpData的内存起始位置
                                               // 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中
            System.Runtime.InteropServices.Marshal.Copy(pixValue, 0, iptr, pixValue.Length);
            mCurrBitmap.UnlockBits(mBitmapData);
            return mCurrBitmap;
        }
    }
}