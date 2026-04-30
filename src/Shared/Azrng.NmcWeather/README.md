# Azrng.NmcWeather

`Azrng.NmcWeather` 是一个基于 `Common.HttpClients` 的中央气象台天气接口客户端。

当前接口按职责拆分为三类：
- `INmcLocationClient`：省份、城市、编码查询
- `INmcWeatherClient`：只负责按城市 `code/stationid` 获取天气
- `INmcWeatherQueryClient`：面向业务的便捷天气查询，支持按城市名等方式获取天气

## 功能

- 获取全部省份列表
- 根据省份名称或编码获取省份信息
- 根据省份名称或编码获取城市列表
- 根据指定省份获取城市编码列表、城市名称列表、城市名称与编码映射
- 根据省份名称快速获取省份编码
- 根据城市名称快速获取城市编码
- 根据城市名称或编码获取城市信息
- 根据城市名称或编码获取天气信息

## 安装

```bash
dotnet add package Azrng.NmcWeather
```

## 注册

```csharp
using Azrng.NmcWeather;

builder.Services.AddNmcWeather(options =>
{
    options.BaseUrl = "http://www.nmc.cn";
});
```

如果容器里还没有注册 `Common.HttpClients`，`AddNmcWeather` 会自动补注册默认的 `IHttpHelper`。

## 使用

```csharp
using Azrng.NmcWeather;

public class WeatherAppService
{
    private readonly INmcLocationClient _locationClient;
    private readonly INmcWeatherClient _weatherClient;
    private readonly INmcWeatherQueryClient _weatherQueryClient;

    public WeatherAppService(
        INmcLocationClient locationClient,
        INmcWeatherClient weatherClient,
        INmcWeatherQueryClient weatherQueryClient)
    {
        _locationClient = locationClient;
        _weatherClient = weatherClient;
        _weatherQueryClient = weatherQueryClient;
    }

    public async Task DemoAsync()
    {
        var province = await _locationClient.GetProvinceAsync("北京");
        var provinceCode = await _locationClient.GetProvinceCodeAsync("北京");
        var cities = await _locationClient.GetCitiesByProvinceAsync("ABJ");
        var cityCodes = await _locationClient.GetCityCodesByProvinceAsync("北京");
        var cityCodeMap = await _locationClient.GetCityCodeMapByProvinceAsync("北京");
        var city = await _locationClient.GetCityAsync("朝阳");
        var cityCode = await _locationClient.GetCityCodeAsync("朝阳", provinceName: "北京");

        var weatherByCode = await _weatherClient.GetWeatherByCityCodeAsync(cityCode!);
        var weatherByName = await _weatherQueryClient.GetWeatherByCityNameAsync("朝阳", provinceName: "北京");
    }
}
```

## 说明

- 默认接口地址使用 `http://www.nmc.cn`
- 当前版本依赖 `Common.HttpClients`，因此目标框架与它保持一致，为 `net6.0` 到 `net10.0`
- 城市名称查询支持常见行政后缀的宽松匹配，例如 `北京` 与 `北京市`
- 真实联调时发现当前线上城市站点编码为区分大小写的混合字符串，例如 `Wqsps`，因此天气查询会保留原始 `stationid` 大小写

## 版本更新记录

### 1.0.0

- 首次发布 `Azrng.NmcWeather`
- 提供 `INmcLocationClient`、`INmcWeatherClient`、`INmcWeatherQueryClient` 三类接口
- 支持省份、城市、编码查询，以及按城市编码获取天气
- 支持按城市名称进行便捷天气查询
- 基于真实联调修正站点编码大小写保留逻辑，兼容当前线上混合大小写 `stationid`
- 补充并验证 `net6.0` 到 `net10.0` 多目标框架构建，其中测试覆盖 `net6.0` 与 `net8.0`
