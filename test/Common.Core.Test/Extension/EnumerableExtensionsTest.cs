namespace Common.Core.Test.Extension
{
    public class EnumerableExtensionsTest
    {
        [Fact]
        public void Enumerable_Null_Return()
        {
            List<int> list = null;
            Assert.True(list.IsNullOrEmpty());
        }

        [Fact]
        public void Enumerable_NotNull_Return()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.True(list.IsNotNullOrEmpty());
        }
    }
}