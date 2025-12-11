using System.ComponentModel;

namespace CommonCollect.Desen.Handles
{
    /// <summary>
    /// 脱敏算法类别
    /// </summary>
    public enum DesenArithmeticType
    {
        /// <summary>
        /// 保留前N后M
        /// </summary>
        [Description("保留前N后M")] RetainFrontNBackM,

        /// <summary>
        /// 保留自x至y
        /// </summary>
        [Description("保留自X至Y")] RetainFrontXToY,

        /// <summary>
        /// 遮盖前n后m
        /// </summary>
        [Description("遮盖前N后M")] CoverFrontNBackM,

        /// <summary>
        /// 遮盖自x至y
        /// </summary>
        [Description("遮盖自X至Y")] CoverFrontXToY,

        /// <summary>
        /// 关键词前遮盖
        /// </summary>
        [Description("关键词前遮盖")] KeywordFrontCover,

        /// <summary>
        /// 关键词后遮盖
        /// </summary>
        [Description("关键词后遮盖")] KeywordBackCover
    }
}