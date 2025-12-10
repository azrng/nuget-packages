
namespace Common.Core.Test.Extension
{
    public class ExceptionExtensionTest
    {
        [Fact]
        public void 记录错误信息_ReturnOk()
        {
            try
            {
                var str = Convert.ToInt32("bb");
            }
            catch (Exception ex)
            {
                var message = ex.GetExceptionAndStack();
            }
        }
    }
}