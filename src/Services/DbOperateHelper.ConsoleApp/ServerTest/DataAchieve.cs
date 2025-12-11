using Azrng.DbOperateHelper;
using System.Data.Common;

namespace DbOperateHelper.ConsoleApp.ServerTest
{
    public class DataAchieve : DataBase
    {
        public override DbConnection CreationConnection()
        {
            return base.CreationConnection(DbType);
        }
    }
}
