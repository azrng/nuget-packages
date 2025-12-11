using MySqlConnector;
using System.Data;
using System.Data.Common;

namespace Azrng.DbOperateHelper
{
    /// <summary>
    /// 资料来自：https://blog.csdn.net/ftfmatlab/article/details/135655836
    /// </summary>
    public class DbHelper
    {
        private readonly DataBase _dataBase;

        public DbHelper(DataBase dataBase)
        {
            _dataBase = dataBase;
        }

        public DataBase GetDataBase()
        {
            return _dataBase;
        }

        public DbConnection GetDbConnection()
        {
            var conn = _dataBase.CreationConnection();

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            return conn;
        }

        /// <summary>
        /// 执行语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public int Execute(string sql, params DbParameter[] cmdParms)
        {
            using var connection = GetDbConnection();
            using var cmd = connection.CreateCommand();
            PrepareCommand(cmd, connection, null, sql, cmdParms);
            var rows = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return rows;
        }

        /// <summary>
        /// 批量查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public DataSet Query(string sql, params DbParameter[] cmdParms)
        {
            using var connection = GetDbConnection();
            var ds = new DataSet();
            var factory = DbProviderFactories.GetFactory(connection);
            var command = factory.CreateCommand();
            PrepareCommand(command, connection, null, sql, cmdParms);
            var adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = command;
            adapter.Fill(ds, "ds");
            adapter.Dispose();
            command.Dispose();

            return ds;
        }

        /// <summary>
        /// 批量查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="reader">数据读取器</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<T> Query<T>(string sql, Func<IDataReader, T> reader, params DbParameter[] cmdParms)
        {
            if (reader == null)
                throw new Exception("数据读取器是空的!");

            var list = new List<T>();
            using var connection = GetDbConnection();
            using var cmd = connection.CreateCommand();
            PrepareCommand(cmd, connection, null, sql, cmdParms);
            DbDataReader myReader = cmd.ExecuteReader();
            cmd.Parameters.Clear();
            while (myReader.Read())
            {
                list.Add(reader(myReader));
            }

            myReader.Close();

            return list;
        }

        /// <summary>
        /// 单个查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="reader">数据读取器</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T QueryFirstOrDefault<T>(string sql, Func<IDataReader, T> reader, params DbParameter[] cmdParms)
        {
            if (reader == null)
            {
                throw new Exception("数据读取器是空的!");
            }

            var model = default(T);
            using var connection = GetDbConnection();
            using var cmd = connection.CreateCommand();
            PrepareCommand(cmd, connection, null, sql, cmdParms);
            var myReader = cmd.ExecuteReader();
            cmd.Parameters.Clear();
            if (myReader.Read())
                model = reader(myReader);

            myReader.Close();

            return model;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns></returns>
        public DataSet RunProcedure(string storedProcName, DbParameter[] parameters)
        {
            using var connection = GetDbConnection();
            var dataSet = new DataSet();
            connection.Open();
            var sqlDa = DbProviderFactories.GetFactory(connection).CreateDataAdapter();
            sqlDa.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
            sqlDa.Fill(dataSet, "ds");
            sqlDa.SelectCommand.Dispose();
            sqlDa.Dispose();
            return dataSet;
        }

        /// <summary>
        /// 执行存储过程，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public DbDataReader RunProcedureToReader(string storedProcName, DbParameter[] parameters)
        {
            using var connection = GetDbConnection();
            connection.Open();
            var command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            var returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            command.Dispose();
            return returnReader;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public T RunProcedure<T>(string storedProcName, Func<IDataReader, T> reader, DbParameter[] parameters)
        {
            if (reader == null)
            {
                throw new Exception("数据读取器是空的!");
            }

            T t = default(T);
            using var connection = GetDbConnection();
            DbDataReader returnReader;
            connection.Open();
            DbCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            command.Dispose();
            if (returnReader.Read())
                t = reader(returnReader);
            returnReader.Close();

            return t;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public List<T> RunProcedureToList<T>(string storedProcName, Func<IDataReader, T> reader,
            DbParameter[] parameters)
        {
            if (reader == null)
            {
                throw new Exception("数据读取器是空的!");
            }

            List<T> list = new List<T>();
            using (DbConnection connection = GetDbConnection())
            {
                DbDataReader returnReader;
                connection.Open();
                DbCommand command = BuildQueryCommand(connection, storedProcName, parameters);
                command.CommandType = CommandType.StoredProcedure;
                returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                command.Dispose();
                while (returnReader.Read())
                    list.Add(reader(returnReader));
                returnReader.Close();
            }

            return list;
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params DbParameter[] cmdParms)
        {
            object result = null;
            using (DbConnection connection = GetDbConnection())
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sql, cmdParms);
                        result = cmd.ExecuteScalar();
                    }
                    catch (DbException e)
                    {
                        throw e;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename">表名(可以自定)</param>
        /// <param name="page">分页信息</param>
        /// <param name="reader">读取器</param>
        /// <param name="where">条件</param>
        /// <param name="field">字段</param>
        /// <param name="order">排序</param>
        public List<T> QueryWithPage<T>(string tablename, PageInfo page, Func<IDataReader, T> reader, string where = "",
            string field = "*", string order = "", params DbParameter[] cmdParms)
        {
            long offset = page.Index * page.PageSize;
            string sql = "SELECT " + field + " FROM " + tablename;
            sql = ListPageSql(sql, where, order);
            sql = sql + " " + Limit(offset, page.PageSize);
            string sql2 = "SELECT COUNT(0) FROM " + tablename;
            sql2 = ListPageSql(sql2, where, "");
            string sql3 = sql + ";" + sql2;
            List<T> list = new List<T>();
            using (DbConnection conn = GetDbConnection())
            {
                using (DbCommand cmd = conn.CreateCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, conn, null, sql3, cmdParms);
                        DbDataReader myReader = cmd.ExecuteReader();
                        cmd.Parameters.Clear();
                        while (myReader.Read())
                        {
                            list.Add(reader(myReader));
                        }

                        if (myReader.NextResult() && myReader.Read())
                            page.Count = myReader.GetInt64Ex(0);

                        myReader.Close();
                    }
                    catch (MySqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 组装分页sql
        /// </summary>
        /// <param name="sql">基础sql</param>
        /// <param name="where">条件</param>
        /// <param name="order">排序</param>
        /// <returns></returns>
        private string ListPageSql(string sql, string where, string order)
        {
            if (!string.IsNullOrEmpty(where))
            {
                sql = sql + " WHERE " + where;
            }

            if (!string.IsNullOrEmpty(order))
            {
                sql = sql + " " + order;
            }

            return sql;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="offset">偏移</param>
        /// <param name="size">每页显示数据尺寸</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string Limit(long offset, long size)
        {
            if (offset == -1)
            {
                if (_dataBase.DbType != DbBaseType.SqlServer)
                {
                    return "LIMIT " + size;
                }
            }
            else
            {
                if (_dataBase.DbType == DbBaseType.MySql)
                {
                    return string.Format("LIMIT {0},{1}", offset, size);
                }

                if (_dataBase.DbType == DbBaseType.PostgreSql || _dataBase.DbType == DbBaseType.Sqlite)
                {
                    return string.Format(" LIMIT {0} OFFSET {1}", size, offset);
                }
            }

            throw new Exception("暂时不支持其它分页语法");
        }

        public DbParameter CreateDbParameter(string parameterName, DbType dbType, object value)
        {
            using (DbConnection connection = GetDbConnection())
            {
                return CreateDbParameter(connection, parameterName, dbType, value);
            }
        }

        public DbParameter CreateDbParameter(DbConnection connection, string parameterName, DbType dbType, object value)
        {
            DbParameter dbParameter = DbProviderFactories.GetFactory(connection).CreateParameter();
            dbParameter.ParameterName = parameterName;
            dbParameter.DbType = dbType;
            dbParameter.Value = value;
            return dbParameter;
        }

        public DbParameter[] CreateDbParameters(params Tuple<string, DbType, object>[] tuples)
        {
            if (tuples == null) return new DbParameter[0];

            DbParameter[] dbParameters = new DbParameter[tuples.Length];
            using (DbConnection connection = GetDbConnection())
            {
                for (int i = 0; i < dbParameters.Length; i++)
                {
                    var tuple = tuples[i];
                    dbParameters[i] = CreateDbParameter(connection, tuple.Item1, tuple.Item2, tuple.Item3);
                }
            }

            return dbParameters;
        }

        protected void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction trans, string cmdText,
            DbParameter[] cmdParms)
        {
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;
            SetParameters(cmd, cmdParms);
        }

        private DbCommand BuildQueryCommand(DbConnection connection, string storedProcName, DbParameter[] parameters)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = storedProcName;
            command.CommandType = CommandType.StoredProcedure;
            SetParameters(command, parameters);
            return command;
        }

        private void SetParameters(DbCommand command, DbParameter[] cmdParms)
        {
            if (cmdParms != null)
            {
                foreach (var parameter in cmdParms)
                {
                    if (
                        (parameter.Direction == ParameterDirection.InputOutput
                         ||
                         parameter.Direction == ParameterDirection.Input)
                        &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }

                    command.Parameters.Add(parameter);
                }
            }
        }
    }
}