namespace Azrng.Core.Requests
{
    /// <summary>
    /// 操作模型，保存登陆用户必要信息。
    /// </summary>
    public class OperatorDto<T>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public T UserId { get; set; } = default!;

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = string.Empty;

        /// <summary>
        /// 是否可以查看所有数据
        /// </summary>
        /// <example>0：仅自己权限 1所有权限</example>
        public int DataPermission { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public int TenantId { get; set; } = 1;

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; } = string.Empty;
    }

    // /// <summary>
    // /// 数据权限
    // /// </summary>
    // public enum DataPermissionEnum
    // {
    //     /// <summary>
    //     /// 仅自己的数据
    //     /// </summary>
    //     [Description("仅自己的数据")]
    //     My = 0,
    //
    //     /// <summary>
    //     /// 查看所有的数据
    //     /// </summary>
    //     [Description("查看所有的数据")]
    //     All = 1
    // }
}