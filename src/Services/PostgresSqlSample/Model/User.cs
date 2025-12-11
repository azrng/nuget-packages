namespace PostgresSqlSample.Model;

/// <summary>
/// 用户信息
/// </summary>
public class User : IdentityBaseEntity
{
    private User() { }

    public User(string account, string passWord, bool isValid) : this()
    {
        Account = account;
        Password = passWord;
        IsValid = isValid;
        UserName = account;
        CreateTime = DateTime.Now.ToUnspecifiedDateTime();
    }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid { get; set; }
}