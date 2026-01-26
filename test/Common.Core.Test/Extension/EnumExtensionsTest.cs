using Common.Core.Test.Model;

namespace Common.Core.Test.Extension
{
    /// <summary>
    /// 枚举扩展测试
    /// </summary>
    public class EnumExtensionsTest
    {
        [Theory]
        [InlineData(ScheduleTimeIntervalType.Morning,"早上")]
        [InlineData(ScheduleTimeIntervalType.Afternoon,"下午")]
        [InlineData(ScheduleTimeIntervalType.AllDay,"全天")]
        [InlineData((ScheduleTimeIntervalType)4,"")]
        public void GetDescription_ReturnOk(ScheduleTimeIntervalType scheduleTimeIntervalType,string description)
        {
            var result = scheduleTimeIntervalType.GetDescription();

            Assert.Equal(description, result);
        }

        [Theory]
        [InlineData(ScheduleTimeIntervalType.Morning,"Morning")]
        [InlineData(ScheduleTimeIntervalType.Afternoon,"Afternoon")]
        [InlineData(ScheduleTimeIntervalType.AllDay,"All Day")]
        [InlineData((ScheduleTimeIntervalType)4,"")]
        public void GetEnglishDescription_ReturnOk(ScheduleTimeIntervalType scheduleTimeIntervalType,string description)
        {
            var result = scheduleTimeIntervalType.GetEnglishDescription();

            Assert.Equal(description, result);
        }
    }
}