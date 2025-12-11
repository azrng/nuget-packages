using Hei.Captcha;

namespace CommonCollect
{
    /// <summary>
    /// 验证码
    /// </summary>
    public class ImageSharpService
    {
        private SecurityCodeHelper _securityCode = new SecurityCodeHelper();

        /// <summary>
        /// 泡泡中文验证码
        /// </summary>
        /// <returns></returns>
        public byte[] BubbleCode()
        {
            var code = _securityCode.GetRandomCnText(2);
            var imgbyte = _securityCode.GetBubbleCodeByte(code);

            return imgbyte;//File(imgbyte, "image/png");
        }

        /// <summary>
        /// 数字字母组合验证码
        /// </summary>
        /// <returns></returns>
        public byte[] HybridCode()
        {
            var code = _securityCode.GetRandomEnDigitalText(4);
            var imgbyte = _securityCode.GetEnDigitalCodeByte(code);

            return imgbyte;//File(imgbyte, "image/png");
        }
    }
}