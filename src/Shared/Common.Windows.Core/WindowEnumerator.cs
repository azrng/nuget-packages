using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.Windows.Core;

/// <summary>
/// windows窗口帮助类
/// </summary>
public class WindowEnumerator
{
    /// <summary>
    /// 检查鼠标坐标是否在指定范围内
    /// </summary>
    /// <param name="mouseX">当前x坐标</param>
    /// <param name="mouseY">当前y坐标</param>
    /// <param name="rangeXStart">开始x坐标</param>
    /// <param name="rangeXEnd">结束x坐标</param>
    /// <param name="rangeYStart">开始y坐标</param>
    /// <param name="rangeYEnd">结束y坐标</param>
    /// <returns></returns>
    public static bool CheckMousePosition(int mouseX, int mouseY, int rangeXStart, int rangeXEnd, int rangeYStart, int rangeYEnd)
    {
        return mouseX >= rangeXStart && mouseX <= rangeXEnd &&
               mouseY >= rangeYStart && mouseY <= rangeYEnd;
    }

    /// <summary>
    /// 根据前缀去匹配窗口是否存在
    /// </summary>
    /// <param name="title">窗口名</param>
    /// <returns></returns>
    public static bool ExistWindowsForTitle(string title)
    {
        var windowList = new List<WindowInfo>();
        EnumWindows(OnWindowEnum, 0);
        return windowList.Any(x => x is { IsVisible: true, Title.Length: > 0 } && x.Title.StartsWith(title));

        bool OnWindowEnum(IntPtr hWnd, int lparam)
        {
            // 仅查找顶层窗口。
            if (GetParent(hWnd) == IntPtr.Zero)
            {
                // 获取窗口类名。
                var lpString = new StringBuilder(512);
                GetClassName(hWnd, lpString, lpString.Capacity);
                var className = lpString.ToString();
                // 获取窗口标题。
                var lptrString = new StringBuilder(512);
                GetWindowText(hWnd, lptrString, lptrString.Capacity);
                var titleTrim = lptrString.ToString().Trim();
                // 获取窗口可见性。
                var isVisible = IsWindowVisible(hWnd);
                // 获取窗口位置和尺寸。
                LPRECT rect = default;
                GetWindowRect(hWnd, ref rect);
                var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                // 添加到已找到的窗口列表。
                windowList.Add(new WindowInfo(hWnd, className, titleTrim, isVisible, bounds));
            }

            return true;
        }
    }

    /// <summary>
    /// 查找当前用户空间下所有符合条件的窗口。如果不指定条件，将仅查找可见窗口。
    /// </summary>
    /// <param name="match">过滤窗口的条件。如果设置为 null，将仅查找可见窗口。</param>
    /// <returns>找到的所有窗口信息。</returns>
    public static IReadOnlyList<WindowInfo> FindAll(Predicate<WindowInfo> match = null)
    {
        var windowList = new List<WindowInfo>();
        EnumWindows(OnWindowEnum, 0);
        return windowList.FindAll(match ?? DefaultPredicate);

        bool OnWindowEnum(IntPtr hWnd, int lparam)
        {
            // 仅查找顶层窗口。
            if (GetParent(hWnd) == IntPtr.Zero)
            {
                // 获取窗口类名。
                var lpString = new StringBuilder(512);
                GetClassName(hWnd, lpString, lpString.Capacity);
                var className = lpString.ToString();
                // 获取窗口标题。
                var lptrString = new StringBuilder(512);
                GetWindowText(hWnd, lptrString, lptrString.Capacity);
                var title = lptrString.ToString().Trim();
                // 获取窗口可见性。
                var isVisible = IsWindowVisible(hWnd);
                // 获取窗口位置和尺寸。
                LPRECT rect = default;
                GetWindowRect(hWnd, ref rect);
                var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                // 添加到已找到的窗口列表。
                windowList.Add(new WindowInfo(hWnd, className, title, isVisible, bounds));
            }

            return true;
        }
    }

    #region 获取窗口信息用的

    /// <summary>
    /// 默认的查找窗口的过滤条件。可见 + 非最小化 + 包含窗口标题。
    /// </summary>
    private static readonly Predicate<WindowInfo> DefaultPredicate =
        x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0;

    private delegate bool WndEnumProc(IntPtr hWnd, int lParam);

    [DllImport("user32")]
    private static extern bool EnumWindows(WndEnumProc lpEnumFunc, int lParam);

    [DllImport("user32")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lptrString, int nMaxCount);

    [DllImport("user32")]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref LPRECT rect);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct LPRECT
    {
        public readonly int Left;
        public readonly int Top;
        public readonly int Right;
        public readonly int Bottom;
    }

    #endregion

    /// <summary>
    /// 获取 Win32 窗口的一些基本信息。
    /// </summary>
    public readonly struct WindowInfo
    {
        public WindowInfo(IntPtr hWnd, string className, string title, bool isVisible, Rectangle bounds) : this()
        {
            Hwnd = hWnd;
            ClassName = className;
            Title = title;
            IsVisible = isVisible;
            Bounds = bounds;
        }

        /// <summary>
        /// 获取窗口句柄。
        /// </summary>
        public IntPtr Hwnd { get; }

        /// <summary>
        /// 获取窗口类名。
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// 获取窗口标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 获取当前窗口是否可见。
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// 获取窗口当前的位置和尺寸。
        /// </summary>
        private Rectangle Bounds { get; }

        /// <summary>
        /// 获取窗口当前是否是最小化的。
        /// </summary>
        public bool IsMinimized => Bounds.Left == -32000 && Bounds.Top == -32000;
    }
}