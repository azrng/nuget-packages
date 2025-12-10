namespace Common.Core.Test.Helper
{
    public class DateTimeHelperTest
    {
        /// <summary>
        /// 计算相隔的天数
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="day"></param>
        [Theory]
        [InlineData("2025-09-17", "2025-09-17", 0)]
        [InlineData("2025-09-17", "2025-09-18", 1)]
        [InlineData("2025-09-18", "2025-09-17", 1)]
        [InlineData("2025-09-17 10:00:00", "2025-09-18 10:00:01", 2)]
        [InlineData("2025-09-17 10:00:00", "2025-09-18 09:00:01", 1)]
        [InlineData(null, "2025-09-18", 0)]
        [InlineData("2025-09-18", null,0)]
        [InlineData("2025-09-10 10:00:00", "2025-09-20 09:00:01", 10)]
        public void CalculateDaysDifference_ReturnOk(string startTime, string endTime, int day)
        {
            var result = DateTimeHelper.CalculateDaysDifference(startTime.ToDateTime(), endTime.ToDateTime());
            Assert.Equal(day, result);
        }
    }
}