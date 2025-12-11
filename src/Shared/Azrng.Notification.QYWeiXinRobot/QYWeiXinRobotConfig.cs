namespace Azrng.Notification.QYWeiXinRobot
{
    /// <summary>
    /// 企业微信机器人配置
    /// </summary>
    public class QyWeiXinRobotConfig
    {
        /// <summary>
        /// baseUrl
        /// </summary>
        public string BaseUrl { get; set; } = "https://qyapi.weixin.qq.com";

        /// <summary>
        /// 推送key  例如：693a91f6-7xxx-4bc4-97a0-0ec2sifa5aaa
        /// </summary>
        public string Key { get; set; }
    }
}