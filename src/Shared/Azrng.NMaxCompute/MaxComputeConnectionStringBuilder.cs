using System.Data.Common;
using System.Text;

namespace Azrng.NMaxCompute;

/// <summary>
/// MaxCompute 连接字符串构建器
/// </summary>
public class MaxComputeConnectionStringBuilder : DbConnectionStringBuilder
{
    private const string UrlKey = "Url";
    private const string AccessIdKey = "AccessId";
    private const string SecretKeyKey = "SecretKey";
    private const string JdbcUrlKey = "JdbcUrl";
    private const string MaxRowsKey = "MaxRows";

    public MaxComputeConnectionStringBuilder()
    {
    }

    public MaxComputeConnectionStringBuilder(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        ConnectionString = connectionString;
    }

    /// <summary>
    /// 接口地址
    /// </summary>
    public string Url
    {
        get => TryGetValue(UrlKey, out var value) ? value.ToString()! : string.Empty;
        set => this[UrlKey] = value;
    }

    /// <summary>
    /// Access ID
    /// </summary>
    public string AccessId
    {
        get => TryGetValue(AccessIdKey, out var value) ? value.ToString()! : string.Empty;
        set => this[AccessIdKey] = value;
    }

    /// <summary>
    /// Secret Key
    /// </summary>
    public string SecretKey
    {
        get => TryGetValue(SecretKeyKey, out var value) ? value.ToString()! : string.Empty;
        set => this[SecretKeyKey] = value;
    }

    /// <summary>
    /// JDBC URL
    /// </summary>
    public string JdbcUrl
    {
        get => TryGetValue(JdbcUrlKey, out var value) ? value.ToString()! : string.Empty;
        set => this[JdbcUrlKey] = value;
    }

    /// <summary>
    /// 最大返回行数
    /// </summary>
    public int MaxRows
    {
        get => TryGetValue(MaxRowsKey, out var value) ? Convert.ToInt32(value) : 1000;
        set => this[MaxRowsKey] = value;
    }

    /// <summary>
    /// 验证连接字符串是否有效
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Url) &&
               !string.IsNullOrWhiteSpace(AccessId) &&
               !string.IsNullOrWhiteSpace(SecretKey) &&
               !string.IsNullOrWhiteSpace(JdbcUrl);
    }

    /// <summary>
    /// 获取连接字符串
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(Url))
            sb.Append($"{UrlKey}={Url};");

        if (!string.IsNullOrEmpty(AccessId))
            sb.Append($"{AccessIdKey}={AccessId};");

        if (!string.IsNullOrEmpty(SecretKey))
            sb.Append($"{SecretKeyKey}={SecretKey};");

        if (!string.IsNullOrEmpty(JdbcUrl))
            sb.Append($"{JdbcUrlKey}={JdbcUrl};");

        if (MaxRows > 0)
            sb.Append($"{MaxRowsKey}={MaxRows};");

        return sb.ToString().TrimEnd(';');
    }

    /// <summary>
    /// 尝试获取值
    /// </summary>
    private bool TryGetValue(string key, out object? value)
    {
        if (base.TryGetValue(key, out var obj))
        {
            value = obj;
            return true;
        }

        value = null;
        return false;
    }
}
