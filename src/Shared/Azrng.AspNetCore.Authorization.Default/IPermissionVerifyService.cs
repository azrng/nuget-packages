namespace Azrng.AspNetCore.Authorization.Default
{
    /// <summary>
    /// 权限服务
    /// </summary>
    public interface IPermissionVerifyService
    {
        /// <summary>
        /// 根据接口路径判断是否授权通过
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> HasPermission(string path);
    }
}