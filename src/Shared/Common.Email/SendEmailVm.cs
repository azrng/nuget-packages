namespace Common.Email
{
    /// <summary>
    /// 接收人请求类
    /// </summary>
    public class ToAccessVm
    {
        /// <summary>
        /// 接收人用户名
        /// </summary>
        public string ToName { get; set; }

        /// <summary>
        /// 接收人地址
        /// </summary>
        public string Address { get; set; }
    }
}