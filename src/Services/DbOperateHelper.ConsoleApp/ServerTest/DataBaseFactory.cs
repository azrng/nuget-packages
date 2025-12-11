using Azrng.DbOperateHelper;

namespace DbOperateHelper.ConsoleApp.ServerTest
{
    public class DataBaseFactory
    {
        private static DataBase _dataBase;
        static DataBaseFactory()
        {
            _dataBase = new DataAchieve();
            _dataBase.DbType = DbBaseType.MySql;// 这里选择你的数据库类型
            _dataBase.ConnectionString = "server=10.3.20.54;user id=jcDbAdmin; password=Admin404@jsjd; database=db_mcsgf; pooling=false";// 这里写你的数据库连接地址
        }
        public static DataBase Db => _dataBase;
    }
}
