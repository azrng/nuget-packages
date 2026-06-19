namespace Azrng.NMaxCompute.Accounts.Signers;

/// <summary>
/// ODPS 请求签名器抽象
/// </summary>
public interface ISigner
{
    /// <summary>
    /// 计算完整 Authorization 头部值
    /// </summary>
    /// <param name="canonicalString">规范化签名串（StringToSign）</param>
    /// <returns>Authorization 头部值，如 <c>ODPS ak:xxx</c> 或 <c>ODPS ak/date/region/odps/aliyun_v4_request:xxx</c></returns>
    string BuildAuthorization(string canonicalString);
}
