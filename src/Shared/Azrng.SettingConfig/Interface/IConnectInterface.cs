using Azrng.SettingConfig.Dto;

namespace Azrng.SettingConfig.Interface;

/// <summary>
/// 业务需要实现的接口
/// </summary>
public interface IConnectInterface
{
    /// <summary>
    /// 配置项业务校验（编辑前回调）
    /// </summary>
    /// <param name="request">更新配置请求</param>
    /// <returns>验证结果，IsOk 为 false 时将中断编辑流程</returns>
    Task<(bool IsOk, string ErrMsg)> ItemValidate(UpdateConfigDetailsRequest request);

    /// <summary>
    /// 应用配置项更新成功后的业务处理
    /// 缓存清除等操作可以在该方法内执行
    /// </summary>
    /// <param name="key">配置key</param>
    /// <param name="value">配置值</param>
    /// <returns>处理结果</returns>
    Task<(bool IsOk, string ErrMsg)> EditSuccessHandle(string key, string value);

    /// <summary>
    /// 应用配置项删除成功后的业务处理
    /// 缓存清除、资源释放等操作可以在该方法内执行
    /// </summary>
    /// <param name="key">配置key</param>
    /// <param name="value">配置值</param>
    /// <returns>处理结果</returns>
    Task<(bool IsOk, string ErrMsg)> DeleteSuccessHandle(string key, string value);
}
