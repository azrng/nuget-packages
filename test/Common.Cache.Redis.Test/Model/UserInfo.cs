namespace Common.Cache.Redis.Test.Model
{
    /// <summary>
    /// 用户信息表
    /// </summary>
    /// <param name="UserName"></param>
    /// <param name="Sex"></param>
    public record class UserInfo(string UserName, int Sex);
}