using System.Security.Principal;

namespace Common.Windows.Core
{
    /// <summary>
    /// windows权限帮助类
    /// </summary>
    public static class WindowsPermissionHelper
    {
        /// <summary>
        /// 是否是管理员权限
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator()
        {
            var current = WindowsIdentity.GetCurrent();
            var windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}