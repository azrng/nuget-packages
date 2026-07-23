# Azrng.NmcWeather

> 中央气象台（NMC）天气接口 .NET 客户端 —— 一行代码接入中国气象数据

![NuGet](https://img.shields.io/nuget/v/Azrng.NmcWeather.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Target](https://img.shields.io/badge/target-.NET%206%20%7C%207%20%7C%208%20%7C%209%20%7C%2010-lightgrey.svg)

📖 项目简介

`Azrng.NmcWeather` 封装了中央气象台（www.nmc.cn）的省份、城市与天气查询接口，为 .NET 应用提供开箱即用的气象数据访问能力。客户端按职责拆分为三类接口：位置查询、天气数据获取、业务级便捷查询，支持按城市名称或编码灵活获取实时天气与预报信息。

目标用户：需要在中国境内应用中集成气象数据的 .NET 开发者。

✨ 特性

*   **三类职责清晰的客户端接口**：`INmcLocationClient`（位置查询）、`INmcWeatherClient`（天气数据）、`INmcWeatherQueryClient`（业务级便捷查询）
*   **城市名称宽松匹配**：支持常见行政后缀容错，如 `北京` 与 `北京市` 均可匹配
*   **多目标框架支持**：同时支持 `net6.0` / `net7.0` / `net8.0` / `net9.0` / `net10.0`
*   **自动依赖注册**：一行 `AddNmcWeather()` 完成 DI 注册，自动补全 `Common.HttpClients` 依赖
*   **异步优先**：全部 API 采用 `async/await`，支持 `CancellationToken` 取消操作

🛠️ 技术栈

*   运行时：.NET 6.0 ~ 10.0（多目标框架）
*   HTTP 客户端：Common.HttpClients（基于 `IHttpClientFactory`）
*   测试框架：xUnit v3 + Moq + Coverlet
*   包管理：NuGet

🚀 快速开始

### 先决条件

确保你的开发环境满足以下要求：

*   .NET SDK 6.0+（推荐 8.0+）

### 安装

```bash
dotnet add package Azrng.NmcWeather
```

### 注册服务

```csharp
using Azrng.NmcWeather;

builder.Services.AddNmcWeather(options =>
{
    options.BaseUrl = "http://www.nmc.cn"; // 默认值，可省略
});
```

> 若容器中尚未注册 `Common.HttpClients`，`AddNmcWeather` 会自动补注册默认的 `IHttpHelper`。

### 使用示例

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

    public async Task DemoAsync(CancellationToken cancellationToken = default)
    {
        // 1. 查询省份信息
        var province = await _locationClient.GetProvinceAsync("北京", cancellationToken);
        var provinceCode = await _locationClient.GetProvinceCodeAsync("北京", cancellationToken);

        // 2. 查询城市列表与编码
        var cities = await _locationClient.GetCitiesByProvinceAsync("ABJ", cancellationToken);
        var cityCode = await _locationClient.GetCityCodeAsync("朝阳", provinceName: "北京", cancellationToken);

        // 3. 按城市编码获取天气
        var weatherByCode = await _weatherClient.GetWeatherByCityCodeAsync(cityCode!, cancellationToken);

        // 4. 按城市名称便捷获取天气（自动解析编码）
        var weatherByName = await _weatherQueryClient.GetWeatherByCityNameAsync("朝阳", provinceName: "北京", cancellationToken);
    }
}
```

## 接口说明

| 接口 | 职责 | 典型方法 |
|---|---|---|
| `INmcLocationClient` | 省份、城市、编码查询 | `GetProvinceAsync`、`GetCityCodeAsync`、`GetCitiesByProvinceAsync` |
| `INmcWeatherClient` | 按城市编码获取天气 | `GetWeatherByCityCodeAsync` |
| `INmcWeatherQueryClient` | 业务级便捷天气查询 | `GetWeatherByCityNameAsync`、`GetWeatherByCityAsync` |

## 配置项

`NmcWeatherOptions` 支持以下配置：

| 属性 | 默认值 | 说明 |
|---|---|---|
| `BaseUrl` | `http://www.nmc.cn` | 中央气象台接口基地址 |
| `ProvincePath` | `/rest/province` | 省份接口路径 |
| `WeatherPath` | `/rest/weather` | 天气接口路径 |

## 注意事项

*   城市站点编码为区分大小写的混合字符串（如 `Wqsps`），天气查询会保留原始大小写
*   城市名称查询支持常见行政后缀的宽松匹配

## 版本更新记录

### 1.1.0

*   **收紧城市编码启发式（破坏性）**：`GetCityAsync` / `GetCityCodeAsync` 等自动判定输入类型的逻辑，由旧规则「长度≥4 且允许字母数字/连字符/下划线」收紧为「长度为 5 且纯 base62（`[A-Za-z0-9]`）」，精确匹配中央气象台站点编码格式。拼音（如 `shanghai`）、英文短词、含连字符/下划线的字符串不再被误判为编码，避免触发全量省份遍历（约 8 秒级）后才回退名称查找
*   **新增配置校验**：`AddNmcWeather` 注册 `NmcWeatherOptionsValidator`，校验 `BaseUrl` / `ProvincePath` / `WeatherPath` 非空白；调用方可通过 `services.AddOptions<NmcWeatherOptions>().ValidateOnStart(...)` 在应用启动期尽早暴露配置错误

### 1.0.0

*   首次发布
*   提供 `INmcLocationClient`、`INmcWeatherClient`、`INmcWeatherQueryClient` 三类接口
*   支持省份、城市、编码查询，以及按城市编码获取天气
*   支持按城市名称进行便捷天气查询
*   兼容 `net6.0` ~ `net10.0` 多目标框架