using Azrng.SettingConfig.Dto;

namespace Azrng.SettingConfig.Interface;

/// <summary>
/// 默认的连接接口实现
/// </summary>
public class DefaultConnectInterface : IConnectInterface
{
    /// <summary>
    /// 配置项业务校验（编辑前回调）
    /// </summary>
    public Task<(bool IsOk, string ErrMsg)> ItemValidate(UpdateConfigDetailsRequest request)
    {
        return Task.FromResult((true, string.Empty));
    }

    /// <summary>
    /// 应用配置项更新成功后的业务处理
    /// </summary>
    public Task<(bool IsOk, string ErrMsg)> EditSuccessHandle(string key, string value)
    {
        return Task.FromResult((true, string.Empty));
    }

    /// <summary>
    /// 应用配置项删除成功后的业务处理
    /// </summary>
    public Task<(bool IsOk, string ErrMsg)> DeleteSuccessHandle(string key, string value)
    {
        return Task.FromResult((true, string.Empty));
    }
}
