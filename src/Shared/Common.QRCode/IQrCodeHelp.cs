namespace Common.QRCode
{
    /// <summary>
    /// 码公共方法
    /// </summary>
    public interface IQrCodeHelp
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns></returns>
        byte[] CreateQrCode(string content, int width = 100, int height = 100);

        /// <summary>
        /// 创建条形码
        /// </summary>
        /// <param name="content">只能数字</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public byte[] CreateBarCode(string content, int width = 100, int height = 100);
    }
}