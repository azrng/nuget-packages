namespace Common.Core.Test.Helper
{
    public class AssemblyHelperTest
    {
        [Fact]
        public void Verify_GetAssemblies_Num()
        {
            var num = AssemblyHelper.GetAssemblies("Common.*.dll");
            Assert.True(num.Length > 1);
        }

        [Fact]
        public void GetAssemblies_ReturnOk()
        {
            var list = AssemblyHelper.GetAllReferencedAssemblies();
            var nameList = list.Select(t => t.FullName).ToList();
            Assert.True(nameList.Count > 0);
        }
    }
}