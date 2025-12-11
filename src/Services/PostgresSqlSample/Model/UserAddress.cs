
namespace PostgresSqlSample.Model
{
    /// <summary>
    /// 用户地址
    /// </summary>
    public class UserAddress : IdentityBaseEntity
    {
        /// <summary>
        /// 地址名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }
    }
}