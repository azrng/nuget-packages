using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using System.Data.Common;

namespace Azrng.DbOperateHelper
{
    public abstract class DataBase
    {
        public DbBaseType DbType { get; set; }
        public string ConnectionString { get; set; }
        protected virtual DbConnection CreationConnection(DbBaseType dbBaseType)
        {
            DbConnection db = null;
            switch (dbBaseType)
            {
                case DbBaseType.SqlServer:
                    db = new SqlConnection(ConnectionString);
                    break;
                case DbBaseType.MySql:
                    db = new MySqlConnection(ConnectionString);
                    break;
                case DbBaseType.Sqlite:
                    db = new SqliteConnection(ConnectionString);
                    break;
                case DbBaseType.PostgreSql:
                    db = new NpgsqlConnection(ConnectionString);
                    break;
            }
            return db;
        }
        public abstract DbConnection CreationConnection();
    }
}
