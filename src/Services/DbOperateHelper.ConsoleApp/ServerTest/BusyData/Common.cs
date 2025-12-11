using Azrng.DbOperateHelper;

namespace DbOperateHelper.ConsoleApp.ServerTest.BusyData
{
    public class Common : DbHelper
    {
        protected string TableName { get; private set; }
        public Common(DataBase dataBase, string tableName) : base(dataBase)
        {
            TableName = tableName;
        }
    }
}
