namespace Common.Email
{
    public class EmailConfig
    {
        /// <summary>
        /// 发件人邮箱指定对应SMTP服务器地址
        /// </summary>
        public string Host { get; set; } = "smtp.163.com";

        /// <summary>
        /// 端口
        /// </summary>
        public int Post { get; set; } = 587;

        /// <summary>
        /// 是否启用加密,默认启用
        /// </summary>
        public bool Ssl { get; set; } = true;

        /// <summary>
        /// 发件人地址
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        ///发件人
        /// </summary>
        public string FromName { get; set; }

        ///// <summary>
        ///// 发件人用户名一般是邮箱地址
        ///// </summary>
        //public string FromUserName { get; set; }

        /// <summary>
        /// 发件人密码（授权码）
        /// </summary>
        public string FromPassword { get; set; }
    }
}