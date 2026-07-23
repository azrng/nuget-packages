using Microsoft.Extensions.Options;

namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台天气客户端配置。
/// </summary>
public class NmcWeatherOptions
{
    /// <summary>
    /// 接口基地址。
    /// </summary>
    public string BaseUrl { get; set; } = "http://www.nmc.cn";

    /// <summary>
    /// 省份接口路径。
    /// </summary>
    public string ProvincePath { get; set; } = "/rest/province";

    /// <summary>
    /// 天气接口路径。
    /// </summary>
    public string WeatherPath { get; set; } = "/rest/weather";
}

/// <summary>
/// <see cref="NmcWeatherOptions"/> 的配置校验器，确保关键路径非空。
/// </summary>
public class NmcWeatherOptionsValidator : IValidateOptions<NmcWeatherOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, NmcWeatherOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            failures.Add("BaseUrl 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(options.ProvincePath))
        {
            failures.Add("ProvincePath 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(options.WeatherPath))
        {
            failures.Add("WeatherPath 不能为空。");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
