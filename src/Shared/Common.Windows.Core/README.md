# Common.Windows.Core

一个用于 Windows 平台的核心工具类库，提供硬件信息获取、网络检测、窗口管理等常用功能的封装。

## 支持的 .NET 版本

- .NET 6.0 Windows
- .NET 7.0 Windows
- .NET 8.0 Windows
- .NET 9.0 Windows

## 安装

```bash
dotnet add package Common.Windows.Core
```

## 功能特性

### 1. 硬件信息获取 (HardwareInfo)

获取 Windows 系统的硬件信息，用于设备标识和软件授权验证。

- 获取主机名
- 获取 CPU 处理器 ID
- 获取主硬盘序列号
- 获取 MAC 地址
- 获取 BIOS 序列号
- 生成设备指纹（SHA256 哈希）
- 检测软件安装状态

### 2. 网络检测 (NetWorkHelper)

检测网络连接状态的两种方式：

- Ping 方式检测（简单易用）
- Win32 API 检测（提供详细信息）

### 3. 键盘鼠标锁定 (BlockKeyboardMouseHelper)

锁定/解锁键盘和鼠标输入（需要管理员权限）。

### 4. Windows 权限检测 (WindowsPermissionHelper)

检测当前进程是否以管理员权限运行。

### 5. 窗口枚举与管理 (WindowEnumerator)

枚举和管理 Windows 窗口：

- 枚举所有可见窗口
- 检查指定窗口是否存在
- 获取窗口详细信息（句柄、类名、标题、位置、尺寸等）
- 检查鼠标位置是否在指定范围内

### 6. Win32 API 封装 (Win32ApiConfigHelper)

封装常用的 Win32 API 调用：

- Netbios API
- 注册表刷新通知
- 网络状态检测

## 使用示例

### 硬件信息获取

```csharp
using Common.Windows.Core;

// 获取主机名
var hostName = HardwareInfo.GetHostName();

// 获取 CPU ID
var cpuId = HardwareInfo.GetCpuId();

// 获取主硬盘序列号
var diskId = HardwareInfo.GetMainDiskId();

// 获取 MAC 地址
var macAddress = HardwareInfo.GetMacAddress();

// 获取 BIOS 序列号
var biosSerial = HardwareInfo.GetBiosSerial();
```

### 生成设备指纹

```csharp
// 使用默认硬件信息生成指纹
var fingerprint = HardwareInfo.GenerateFingerprint();

// 使用自定义信息生成指纹
var customFingerprint = HardwareInfo.GenerateFingerprint("customData1", "customData2");
```

### 检测软件安装

```csharp
// 检测是否安装了某个软件，并返回可执行文件路径
if (HardwareInfo.CheckInstall("Visual Studio", "devenv.exe", out string installPath))
{
    Console.WriteLine($"软件已安装，路径: {installPath}");
}
```

### 网络检测

```csharp
// 使用 Ping 方式检测网络
bool isOnline = NetWorkHelper.PingCheckNet();

// 使用 Win32 API 检测网络
bool isAvailable = NetWorkHelper.IsNetworkAvailable();
```

### 键盘鼠标锁定

```csharp
// 锁定键盘和鼠标（需要管理员权限）
bool locked = BlockKeyboardMouseHelper.Off();

// 解锁键盘和鼠标
bool unlocked = BlockKeyboardMouseHelper.On();
```

### 权限检测

```csharp
// 检查是否以管理员权限运行
if (WindowsPermissionHelper.IsAdministrator())
{
    Console.WriteLine("当前具有管理员权限");
}
else
{
    Console.WriteLine("当前不是管理员权限");
}
```

### 窗口枚举

```csharp
// 获取所有可见窗口
var windows = WindowEnumerator.FindAll();

foreach (var window in windows)
{
    Console.WriteLine($"标题: {window.Title}");
    Console.WriteLine($"类名: {window.ClassName}");
    Console.WriteLine($"句柄: {window.Hwnd}");
    Console.WriteLine($"可见: {window.IsVisible}");
    Console.WriteLine($"最小化: {window.IsMinimized}");
}

// 检查指定标题的窗口是否存在
bool exists = WindowEnumerator.ExistWindowsForTitle("Notepad");

// 检查鼠标位置是否在指定范围内
bool inRange = WindowEnumerator.CheckMousePosition(mouseX, mouseY, 0, 100, 0, 100);

// 使用自定义条件查找窗口
var specificWindows = WindowEnumerator.FindAll(w =>
    w.Title.Contains("Chrome") && w.IsVisible);
```

## 依赖项

- `System.Management` - 用于 WMI 查询获取硬件信息
- `Azrng.Core` - 核心工具库（提供扩展方法和日志帮助）

## 注意事项

1. **硬件信息获取**
   - 部分 WMI 查询可能需要管理员权限
   - 虚拟机环境下的硬件信息可能与物理机不同
   - 设备指纹包含内置盐值，确保唯一性

2. **网络检测**
   - Ping 方式可能受防火墙影响
   - Win32 API 方式在某些环境下可能不准确

3. **键盘鼠标锁定**
   - 必须以管理员权限运行
   - 某些场景下可能失效（如 Ctrl+Alt+Del）
   - 使用时需谨慎，避免无法解锁

4. **窗口枚举**
   - 仅枚举顶层窗口
   - 默认过滤不可见和最小化的窗口

## 版本历史

### 0.0.6 (最新)
- 优化生成指纹的方法

### 0.0.5
- 更新生成指纹的方法

### 0.0.4
- 信息获取增加异常处理

### 0.0.3
- 优化设备信息获取
- 增加生成设备指纹方法

### 0.0.2
- 支持 .NET 7、.NET 8、.NET 9

### 0.0.1
- 初始版本
- 仅支持 .NET 6

## 许可证

版权归 Azrng 所有
