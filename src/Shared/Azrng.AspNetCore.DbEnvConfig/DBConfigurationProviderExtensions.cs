using Microsoft.Extensions.Configuration;

namespace Azrng.AspNetCore.DbEnvConfig;

public static class DbConfigurationProviderExtensions
{
    /// <summary>
    /// 添加数据库配置系统
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="action"></param>
    /// <param name="scriptService"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddDbConfiguration(this IConfigurationBuilder builder,
                                                           Action<DBConfigOptions> action, IScriptService? scriptService = null)
    {
        var setup = new DBConfigOptions();
        action(setup);

        setup.ParamVerify();
        return builder.Add(new DbConfigurationSource(setup, scriptService ?? new DefaultScriptService()));
    }
}