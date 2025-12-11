using CommonCollect.Desen.Handles;
using System;
using System.Collections.Generic;

namespace CommonCollect.Desen
{
    /// <summary>
    /// 脱敏服务工厂
    /// </summary>
    public static class DesenArithmeticFactory
    {
        /// <summary>
        /// 处理程序字典
        /// </summary>
        private static readonly Dictionary<DesenArithmeticType, IDesenHandle> _desenHandles = new()
        {
            { DesenArithmeticType.RetainFrontNBackM, new RetainFrontNBackMHandle() },
            { DesenArithmeticType.RetainFrontXToY, new RetainFrontXToYHandle() },
            { DesenArithmeticType.CoverFrontNBackM, new CoverFrontNBackMHandle() },
            { DesenArithmeticType.CoverFrontXToY, new CoverFrontXToYHandle() },
            { DesenArithmeticType.KeywordFrontCover, new KeywordFrontCoverHandle() },
            { DesenArithmeticType.KeywordBackCover, new KeywordBackCoverHandle() }
        };

        /// <summary>
        /// 获取脱敏服务
        /// </summary>
        /// <param name="desenArithmeticType"></param>
        /// <returns></returns>
        public static IDesenHandle GetDesenService(DesenArithmeticType desenArithmeticType)
        {
            if (_desenHandles.TryGetValue(desenArithmeticType, out var handle))
            {
                return handle;
            }

            throw new ArgumentOutOfRangeException(nameof(desenArithmeticType), desenArithmeticType, null);
        }
    }
}