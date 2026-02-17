using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace Azrng.SettingConfig.BasicAuthorization;

/// <summary>
/// Basic 认证授权用户
/// </summary>
public class BasicAuthAuthorizationUser
{
    private const int SaltSize = 16; // 128 bit salt
    private const int HashSize = 32; // 256 bit hash
    private const int Iterations = 10000; // PBKDF2 iterations

    /// <summary>
    /// 获取或设置用户登录名
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置密码的哈希值（包含盐值）
    /// 格式: [salt(16 bytes)][hash(32 bytes)]
    /// </summary>
    public byte[]? Password { get; set; }

    /// <summary>
    /// 设置明文密码，自动转换为 PBKDF2 哈希值（带盐值）
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? PasswordClear
    {
        set
        {
            if (value is null)
            {
                Password = null;
                return;
            }

            // 生成随机盐值
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // 使用 PBKDF2 生成哈希
            using var pbkdf2 = new Rfc2898DeriveBytes(value, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            // 组合盐值和哈希值: [salt][hash]
            Password = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, Password, 0, SaltSize);
            Array.Copy(hash, 0, Password, SaltSize, HashSize);
        }
    }

    /// <summary>
    /// 验证用户凭据
    /// </summary>
    /// <param name="login">用户登录名</param>
    /// <param name="password">用户密码</param>
    /// <param name="loginCaseSensitive">登录名是否区分大小写</param>
    /// <returns>验证成功返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentNullException">
    /// 当 <paramref name="login"/> 为 null 或空白时抛出
    /// 或当 <paramref name="password"/> 为 null 或空白时抛出
    /// </exception>
    public bool Validate(string login, string password, bool loginCaseSensitive)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentNullException(nameof(login));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        if (Password is null || Password.Length != SaltSize + HashSize)
            return false;

        var comparison = loginCaseSensitive
            ? StringComparison.CurrentCulture
            : StringComparison.OrdinalIgnoreCase;

        if (login.Equals(Login, comparison))
        {
            // 从存储的密码中提取盐值和哈希值
            var salt = new byte[SaltSize];
            var storedHash = new byte[HashSize];
            Array.Copy(Password, 0, salt, 0, SaltSize);
            Array.Copy(Password, SaltSize, storedHash, 0, HashSize);

            // 使用相同的盐值计算输入密码的哈希
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var testHash = pbkdf2.GetBytes(HashSize);

            // 使用恒定时间比较，防止时序攻击
            return CryptographicOperations.FixedTimeEquals(testHash, storedHash);
        }

        return false;
    }
}