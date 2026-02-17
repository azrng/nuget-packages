# Common.Windows.Core 架构与原理说明

## 项目概述

Common.Windows.Core 是一个专门为 Windows 平台设计的 .NET 工具类库，通过封装 Windows API 和 WMI (Windows Management Instrumentation) 接口，提供硬件信息获取、网络检测、窗口管理等常用功能。

## 设计目标

1. **简化 Windows API 调用** - 将复杂的 Win32 API 和 WMI 查询封装为简单的静态方法
2. **跨版本兼容** - 支持 .NET 6/7/8/9 多个版本
3. **统一异常处理** - 提供一致的错误处理机制
4. **可扩展性** - 模块化设计，各功能独立

## 项目结构

```
Common.Windows.Core/
├── HardwareInfo.cs              # 硬件信息获取
├── NetWorkHelper.cs             # 网络连接检测
├── BlockKeyboardHelper.cs       # 键盘鼠标锁定（类名实际为 BlockKeyboardMouseHelper）
├── WindowsPermissionHelper.cs   # Windows 权限检测
├── Win32APIConfigHelper.cs      # Win32 API 底层封装
├── WindowEnumerator.cs          # 窗口枚举与管理
├── WindowsConfigConst.cs        # 配置常量
└── Common.Windows.Core.csproj   # 项目配置
```

## 核心模块设计

### 1. 硬件信息模块 (HardwareInfo)

#### 架构设计

```
HardwareInfo
    ├── WMI 查询层
    │   ├── ManagementObjectSearcher
    │   └── ManagementObject
    ├── 数据提取层
    │   ├── GetHostName()
    │   ├── GetCpuId()
    │   ├── GetMainDiskId()
    │   ├── GetMacAddress()
    │   └── GetBiosSerial()
    └── 应用层
        ├── GenerateFingerprint()
        └── CheckInstall()
```

#### 技术原理

**WMI (Windows Management Instrumentation)**

WMI 是 Windows 管理服务的核心组件，提供了统一的系统管理接口。本项目通过 `System.Management` 命名空间访问 WMI。

```csharp
// WMI 查询示例
using var searcher = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
using var items = searcher.Get();
```

**常用 WMI 类**

| WMI 类 | 用途 | 查询字段 |
|--------|------|----------|
| Win32_processor | CPU 信息 | ProcessorId |
| Win32_DiskDrive | 硬盘信息 | SerialNumber |
| Win32_NetworkAdapter | 网络适配器 | MACAddress |
| Win32_BIOS | BIOS 信息 | SerialNumber |

#### 设备指纹生成

```
指纹生成流程:
┌─────────────────────────────────────────────────────────┐
│ 1. 收集硬件信息                                         │
│    - CPU ID + BIOS序列号 + 硬盘序列号 + MAC地址         │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ 2. 添加自定义数据（可选）                               │
│    - 用户可传入自定义字符串数组                         │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ 3. 混合盐值                                             │
│    - WindowsConfigConst.Salt (防止彩虹表攻击)           │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ 4. SHA256 哈希计算                                      │
│    - 使用 SHA256.Create() 计算哈希值                   │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ 5. 格式化输出                                           │
│    - 转换为十六进制字符串，去除分隔符                   │
└─────────────────────────────────────────────────────────┘
```

**代码实现**

```csharp
public static string GenerateFingerprint(params string[] values)
{
    var components = new StringBuilder();

    // 收集硬件信息或使用自定义值
    if (values.Length > 0)
    {
        foreach (var value in values)
            components.Append(value);
    }
    else
    {
        components.Append(GetCpuId());
        components.Append(GetBiosSerial());
        components.Append(GetMainDiskId());
        components.Append(GetMacAddress());
    }

    // 添加盐值
    components.Append(WindowsConfigConst.Salt);

    // 计算哈希
    using var sha256 = SHA256.Create();
    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(components.ToString()));

    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
}
```

#### 软件安装检测

通过查询 Windows 注册表来检测软件是否已安装：

```
注册表路径:
├── HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\
│   └── Microsoft\Windows\CurrentVersion\Uninstall  (32位软件)
└── HKEY_LOCAL_MACHINE\SOFTWARE\
    └── Microsoft\Windows\CurrentVersion\Uninstall  (64位软件)
```

**检测流程**

```
1. 刷新注册表 (SHChangeNotify)
2. 遍历卸载注册表项
3. 读取 DisplayName 匹配软件名称
4. 读取 InstallLocation 获取安装路径
5. 验证可执行文件是否存在
6. 返回完整可执行文件路径
```

### 2. 网络检测模块 (NetWorkHelper)

#### 架构设计

```
NetWorkHelper
    ├── Ping 方式
    │   ├── System.Net.NetworkInformation.Ping
    │   └── 发送 ICMP 请求到目标主机
    └── Win32 API 方式
        └── InternetGetConnectedState (wininet.dll)
```

#### 技术原理

**方式一：Ping 检测**

```csharp
public static bool PingCheckNet()
{
    try
    {
        var ping = new Ping();
        var pr = ping.Send("www.baidu.com");
        return pr.Status == IPStatus.Success;
    }
    catch
    {
        return false;
    }
}
```

- **优点**：简单易用，无需特殊权限
- **缺点**：可能受防火墙影响，目标主机可能禁 ping

**方式二：Win32 API 检测**

```csharp
[DllImport("wininet.dll")]
public static extern bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

public static bool IsNetworkAvailable()
{
    return Win32ApiConfigHelper.InternetGetConnectedState(out _, 0);
}
```

- **优点**：提供更详细的网络连接信息
- **缺点**：在某些环境下可能不准确

### 3. 键盘鼠标锁定模块 (BlockKeyboardMouseHelper)

#### 架构设计

```
BlockKeyboardMouseHelper
    ├── 权限检查
    │   └── WindowsPermissionHelper.IsAdministrator()
    └── Win32 API 调用
        └── BlockInput (user32.dll)
```

#### 技术原理

通过调用 user32.dll 中的 `BlockInput` 函数实现：

```csharp
[DllImport("user32.dll", EntryPoint = "BlockInput")]
private static extern bool BlockInput(bool fBlockIt);
```

**使用限制**

1. **必须以管理员权限运行** - 否则调用失败
2. **Ctrl+Alt+Del 无法阻止** - Windows 安全组合键始终有效
3. **谨慎使用** - 避免程序崩溃导致无法解锁

### 4. 权限检测模块 (WindowsPermissionHelper)

#### 技术原理

通过 .NET 的 Windows 身份验证 API 检测：

```csharp
public static bool IsAdministrator()
{
    var current = WindowsIdentity.GetCurrent();
    var windowsPrincipal = new WindowsPrincipal(current);
    return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
}
```

**检测流程**

```
1. 获取当前 Windows 身份标识
2. 创建 WindowsPrincipal 对象
3. 检查是否属于 Administrator 角色
4. 返回检测结果
```

### 5. 窗口枚举模块 (WindowEnumerator)

#### 架构设计

```
WindowEnumerator
    ├── Win32 API 层
    │   ├── EnumWindows() - 枚举窗口
    │   ├── GetWindowText() - 获取窗口标题
    │   ├── GetClassName() - 获取窗口类名
    │   ├── IsWindowVisible() - 检测可见性
    │   └── GetWindowRect() - 获取窗口位置和尺寸
    ├── 数据结构层
    │   └── WindowInfo (窗口信息结构体)
    └── 应用层
        ├── FindAll() - 查找所有窗口
        ├── ExistWindowsForTitle() - 检查窗口是否存在
        └── CheckMousePosition() - 检查鼠标位置
```

#### 技术原理

**窗口枚举流程**

```
1. 调用 EnumWindows() 开始枚举
2. 对每个窗口调用回调函数
3. 过滤顶层窗口 (GetParent() == IntPtr.Zero)
4. 获取窗口信息
5. 根据条件过滤窗口
6. 返回窗口列表
```

**核心 API**

```csharp
// 枚举所有顶层窗口
[DllImport("user32")]
private static extern bool EnumWindows(WndEnumProc lpEnumFunc, int lParam);

// 获取窗口句柄的父窗口
[DllImport("user32")]
private static extern IntPtr GetParent(IntPtr hWnd);

// 获取窗口标题
[DllImport("user32")]
private static extern int GetWindowText(IntPtr hWnd, StringBuilder lptrString, int nMaxCount);

// 获取窗口类名
[DllImport("user32")]
private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

// 检测窗口是否可见
[DllImport("user32")]
private static extern bool IsWindowVisible(IntPtr hWnd);

// 获取窗口位置和尺寸
[DllImport("user32")]
private static extern bool GetWindowRect(IntPtr hWnd, ref LPRECT rect);
```

**WindowInfo 结构体**

```csharp
public readonly struct WindowInfo
{
    public IntPtr Hwnd { get; }        // 窗口句柄
    public string ClassName { get; }   // 窗口类名
    public string Title { get; }       // 窗口标题
    public bool IsVisible { get; }     // 是否可见
    private Rectangle Bounds { get; }  // 位置和尺寸
    public bool IsMinimized =>         // 是否最小化
        Bounds.Left == -32000 && Bounds.Top == -32000;
}
```

**默认过滤条件**

```csharp
private static readonly Predicate<WindowInfo> DefaultPredicate =
    x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0;
```

### 6. Win32 API 封装模块 (Win32ApiConfigHelper)

#### 设计目的

统一封装 Win32 API 调用，避免其他模块直接处理 P/Invoke 细节。

#### 封装的 API

| API | DLL | 用途 |
|-----|-----|------|
| Netbios | NETAPI32.DLL | 获取 MAC 地址（备用方法） |
| SHChangeNotify | shell32.dll | 刷新注册表缓存 |
| InternetGetConnectedState | wininet.dll | 检测网络连接状态 |

#### 数据结构

**Ncb (Network Control Block)**

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct Ncb
{
    public byte ncb_command;      // 命令码
    public byte ncb_retcode;      // 返回码
    public byte ncb_lsn;          // 本地会话号
    public byte ncb_num;          // 网络号
    public IntPtr ncb_buffer;     // 缓冲区指针
    public ushort ncb_length;     // 缓冲区长度
    // ... 其他字段
}
```

**AdapterStatus**

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct AdapterStatus
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] adapter_address;  // MAC 地址
    // ... 其他状态信息
}
```

### 7. 配置常量模块 (WindowsConfigConst)

```csharp
public class WindowsConfigConst
{
    /// <summary>
    /// 盐值 - 用于设备指纹生成，防止彩虹表攻击
    /// </summary>
    internal const string Salt = "k5#F$2pX9LmQ8vR6tYw1zB3cD7eE4gH";
}
```

## 依赖关系图

```
┌─────────────────────────────────────────────────────────┐
│                  Common.Windows.Core                     │
└─────────────────────────────────────────────────────────┘
                            │
                            ├── System.Management (WMI)
                            │
                            ├── System.Runtime.InteropServices (P/Invoke)
                            │
                            ├── System.Net.NetworkInformation (Ping)
                            │
                            ├── System.Security.Principal (权限检测)
                            │
                            ├── Microsoft.Win32 (注册表)
                            │
                            └── Azrng.Core (扩展方法、日志)
                                      ├── Extension
                                      └── Helpers
                                          └── LocalLogHelper
```

## 技术栈

| 技术 | 用途 |
|------|------|
| **C#** | 主要开发语言 |
| **.NET 6/7/8/9** | 目标框架 |
| **P/Invoke** | 调用 Win32 API |
| **WMI** | 硬件信息查询 |
| **SHA256** | 设备指纹哈希 |
| **注册表** | 软件安装检测 |

## 设计模式

### 1. 静态工具类模式

所有功能类都使用静态方法，无需实例化：

```csharp
public static class HardwareInfo
{
    public static string GetCpuId() { ... }
}
```

**优点**：
- 简单易用
- 无状态管理
- 性能开销小

### 2. 外观模式 (Facade Pattern)

通过 `Win32ApiConfigHelper` 统一封装底层 API 调用：

```
高层模块 → Win32ApiConfigHelper → Win32 API
```

### 3. 结构体模式 (Struct Pattern)

使用只读结构体存储窗口信息：

```csharp
public readonly struct WindowInfo { ... }
```

**优点**：
- 值类型，无堆分配
- 线程安全
- 不可变性

## 异常处理策略

### 统一异常处理模式

```csharp
try
{
    // 执行操作
    using var searcher = new ManagementObjectSearcher(query);
    // ...
}
catch (Exception ex)
{
    LocalLogHelper.LogError($"message：{ex.GetExceptionAndStack()}");
    return defaultValue;  // 返回安全的默认值
}
```

**处理原则**：
1. 捕获所有异常，避免程序崩溃
2. 记录错误日志
3. 返回安全的默认值（如空字符串、false 等）
4. 不向上抛出异常

## 性能优化

### 1. 使用 using 语句释放资源

```csharp
using var searcher = new ManagementObjectSearcher(query);
using var items = searcher.Get();
```

### 2. 延迟执行

```csharp
// LINQ 延迟执行，避免不必要的内存分配
return items.Cast<ManagementObject>()
            .Select(x => x["ProcessorId"]?.ToString())
            .FirstOrDefault() ?? string.Empty;
```

### 3. 结构体代替类

`WindowInfo` 使用结构体而非类，减少堆内存分配。

## 安全考虑

### 1. 盐值保护

设备指纹生成使用内置盐值，防止彩虹表攻击：

```csharp
components.Append(WindowsConfigConst.Salt);
```

### 2. 权限检查

敏感操作（如键盘锁定）前检查权限：

```csharp
if (!WindowsPermissionHelper.IsAdministrator())
{
    return false;
}
```

### 3. 异常隔离

所有硬件信息获取都包含异常处理，避免信息泄露。

## 扩展性设计

### 添加新硬件信息获取方法

```csharp
public static string GetMotherboardSerial()
{
    try
    {
        using var searcher = new ManagementObjectSearcher(
            "Select SerialNumber From Win32_BaseBoard");
        using var items = searcher.Get();
        return items.Cast<ManagementObject>()
                    .Select(x => x["SerialNumber"]?.ToString())
                    .FirstOrDefault() ?? string.Empty;
    }
    catch (Exception ex)
    {
        LocalLogHelper.LogError($"message：{ex.GetExceptionAndStack()}");
        return string.Empty;
    }
}
```

### 添加新的 Win32 API 封装

```csharp
[DllImport("user32.dll")]
public static extern bool YourNewApi(int parameter);
```

## 最佳实践

### 1. 始终检查返回值

```csharp
var cpuId = HardwareInfo.GetCpuId();
if (string.IsNullOrEmpty(cpuId))
{
    // 处理获取失败的情况
}
```

### 2. 敏感操作前检查权限

```csharp
if (WindowsPermissionHelper.IsAdministrator())
{
    BlockKeyboardMouseHelper.Off();
}
```

### 3. 使用自定义指纹参数

```csharp
// 针对特定应用场景生成指纹
var appFingerprint = HardwareInfo.GenerateFingerprint(
    GetCpuId(),
    "MyAppSpecificSalt"
);
```

## 未来扩展方向

1. **更多硬件信息** - 主板序列号、显卡信息等
2. **更多窗口操作** - 窗口置顶、最小化、最大化等
3. **进程管理** - 进程枚举、启动、关闭等
4. **注册表操作** - 更完善的注册表读写封装
5. **系统服务管理** - 服务安装、启动、停止等

## 参考资料

- [WMI Reference](https://docs.microsoft.com/en-us/windows/win32/wmisdk/wmi-reference)
- [Win32 API Reference](https://docs.microsoft.com/en-us/windows/win32/api/)
- [P/Invoke Documentation](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
