namespace Azrng.AspNetCore.Inject;

/// <summary>
/// 注册模式
/// </summary>
public enum InjectScheme
{
    /// <summary>
    /// 注入父类、接口
    /// </summary>
    Any,

    /// <summary>
    /// 手动选择要注入的服务
    /// </summary>
    Some,

    /// <summary>
    /// 只注入父类
    /// </summary>
    OnlyBaseClass,

    /// <summary>
    /// 只注入实现的接口
    /// </summary>
    OnlyInterfaces,

    /// <summary>
    /// 此服务不会被注入到容器中
    /// </summary>
    None
}