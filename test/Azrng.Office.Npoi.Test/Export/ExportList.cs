using Azrng.Core.Extension;
using Azrng.Office.NPOI;
using Azrng.Office.NPOI.Model;
using Azrng.Office.Npoi.Test.Export.Dtos;
using Xunit.Abstractions;

namespace Azrng.Office.Npoi.Test.Export
{
    public class ExportList
    {
        private readonly ITestOutputHelper _output;
        private readonly List<User> _users;

        public ExportList(ITestOutputHelper output)
        {
            _output = output;
            _users = Enumerable.Range(0, 10)
                               .Select(x => new User { Age = x, Hobby = "Hobby" + x, Name = "张三" + x, Sex = "Sex" + x })
                               .ToList();
        }

        [Fact]
        public void Export_Standard_User()
        {
            var workbookWrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
            var sheet = workbookWrapper.CreateSheet("测试");

            sheet.AddList(_users, new ExportSheetTitle("用户列表", 40));

            var fileName = $"{DateTime.Now.GetTimestamp()}.xlsx";
            using var outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            workbookWrapper.WriteStream(outStream);
        }

        [Fact]
        public void Export_CrateTitle_CreateTable()
        {
            var workbookWrapper = ExcelHelper.CreateWorkbook(ExcelFileType.Xlsx);
            var sheet = workbookWrapper.CreateSheet("测试");

            sheet.AddTitle(new ExportSheetTitle("用户列表"))
                 .AddTitle(new ExportSheetTitle("用户列表"), 0, 2)
                 .AddList(_users);

            sheet.AddCell("总金额：", 1, 1, sheet.NextY)
                 .AddCell("000", 2, 2, sheet.NextY - 1);

            sheet.AddTitle(new ExportSheetTitle("平安喜乐"), 0, 2);
            var fileName = $"{DateTime.Now.GetTimestamp()}.xlsx";
            using var outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            workbookWrapper.WriteStream(outStream);
        }
    }
}