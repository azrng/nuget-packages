using Azrng.Core.Model;
using System.Collections.Generic;

namespace Azrng.EFCore
{
    /// <summary>
    /// EFCore公共配置
    /// </summary>
    public static class EfCoreGlobalConfig
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public static DatabaseType DbType { get; private set; }

        /// <summary>
        /// 表对应的模式
        /// </summary>
        internal static string Schema { get; private set; }

        /// <summary>
        /// 使用老的更新列
        /// </summary>
        internal static bool UseOldUpdateColumn { get; private set; }

        /// <summary>
        /// 老的创建人
        /// </summary>
        internal const string OldCreator = "creater";

        /// <summary>
        /// 老的更新人
        /// </summary>
        internal const string OldUpdater = "modifyer";

        /// <summary>
        /// 老的更新时间
        /// </summary>
        internal const string OldUpdateTime = "modify_time";

        /// <summary>
        /// 存储本次请求过程中创建的仓储类
        /// </summary>
        internal static Dictionary<string, object> Repositories { get; private set; } =
            new Dictionary<string, object>();

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="databaseType"></param>
        /// <param name="schema"></param>
        /// <param name="useOldUpdateColumn"></param>
        public static void SetConfig(DatabaseType databaseType, bool useOldUpdateColumn, string schema = "")
        {
            DbType = databaseType;
            UseOldUpdateColumn = useOldUpdateColumn;
            if (databaseType == DatabaseType.PostgresSql && !string.IsNullOrWhiteSpace(schema))
                Schema = schema;
        }
    }
}