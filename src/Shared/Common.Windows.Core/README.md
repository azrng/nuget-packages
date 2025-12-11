## Common.Windows.Core

## 使用

### 获取设备信息

```c#
// 获取设备指纹
var fingerprint = HardwareInfo.GenerateFingerprint();
```
### 版本更新记录

* 0.0.6
  * 优化生成指纹的方法
* 0.0.5
  * 更新生成指纹的方法
* 0.0.4
  * 信息获取增加异常处理
* 0.0.3
  * 优化设备信息获取
  * 增加生成设备指纹方法
* 0.0.2
    * 支持.net7、.net8、.net9
* 0.0.1
    * 只支持.net6