using Common.Core.Test.Model;

namespace Common.Core.Test.Extension
{
    /// <summary>
    /// 枚举扩展测试
    /// </summary>
    public class EnumExtensionsTest
    {
        [Fact]
        public void GetDescription_ReturnOk()
        {
            var description = "下午";
            var type = ScheduleTimeIntervalType.Afternoon;

            var result = type.GetDescription();

            Assert.Equal(description, result);
        }
    }
}