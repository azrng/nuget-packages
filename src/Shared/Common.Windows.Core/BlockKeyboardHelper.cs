using System.Runtime.InteropServices;

namespace Common.Windows.Core;

/// <summary>
/// 鼠标键盘锁定帮助类(需要管理员模式启动)
/// </summary>
/// <remarks>但是貌似有些场景会失效</remarks>
public static class BlockKeyboardMouseHelper
{
    /// <summary>
    /// 锁定鼠标及键盘
    /// </summary>
    /// <returns></returns>
    public static bool Off()
    {
        if (!WindowsPermissionHelper.IsAdministrator())
        {
            return false;
        }

        BlockInput(true);
        return true;
    }

    /// <summary>
    /// 解锁鼠标及键盘
    /// </summary>
    /// <returns></returns>
    public static bool On()
    {
        if (!WindowsPermissionHelper.IsAdministrator())
        {
            return false;
        }

        BlockInput(false);
        return true;
    }

    // //锁定键盘和鼠标
    // [DllImport("user32.dll")]
    // static extern void BlockInput(bool Block);

    /// <summary>
    /// 锁定键盘以及鼠标
    /// </summary>
    /// <param name="fBlockIt"></param>
    /// <returns></returns>
    [DllImport("user32.dll", EntryPoint = "BlockInput")]
    private static extern bool BlockInput(bool fBlockIt);
}