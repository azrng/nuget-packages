//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using hnstoauto.Utilities;
//using MySql.Data.MySqlClient;

//namespace hnstoauto.DbUtilities
//{
//    public abstract class MysqlHelper
//    {
//        public static string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

//        /// <summary>
//        /// 构造器
//        /// </summary>
//        public MysqlHelper() { }

//        #region 执行简单的SQL语句

//        /// <summary>
//        /// 打开连接
//        /// </summary>
//        /// <param name="conn"></param>
//        public static void Open(MySqlConnection conn)
//        {
//            try
//            {
//                if (conn.State != ConnectionState.Open)
//                {
//                    conn.Open();
//                }
//            }
//            catch (MySqlException e)
//            {
//                //throw new Common.Utilities.Exceptions.DatabaseNoAccessException(e.Message, "ZY.Infrastructure.DbUtilities/MySqlHelper/Open()");
//            }
//        }

//        /// <summary>
//        /// 关闭连接
//        /// </summary>
//        /// <param name="conn"></param>
//        public static void Close(MySqlConnection conn)
//        {
//            try
//            {
//                if (conn.State != ConnectionState.Closed)
//                {
//                    conn.Close();
//                }
//            }
//            catch { }
//        }

//        /// <summary>
//        /// 执行SQL语句，返回影响的记录数
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <returns></returns>
//        public static int ExecuteSql(string sql)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
//                {
//                    Open(conn);

//                    try
//                    {
//                        return cmd.ExecuteNonQuery();
//                    }
//                    catch (MySqlException e)
//                    {
//                        //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/ExecuteSql({0})", sql));
//                        return 0;
//                    }
//                    finally
//                    {
//                        Close(conn);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 执行SQL语句，返回影响的记录数
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="times"></param>
//        /// <returns></returns>
//        public static int ExecuteSql(string sql, int times)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
//                {
//                    Open(conn);

//                    try
//                    {
//                        cmd.CommandTimeout = times;
//                        return cmd.ExecuteNonQuery();
//                    }
//                    catch (MySqlException e)
//                    {
//                        //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/ExecuteSql({0},{1})", sql, times));
//                        return 0;
//                    }
//                    finally
//                    {
//                        Close(conn);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 执行多条SQL语句，实现数据库事务。
//        /// </summary>
//        /// <param name="sqlList">多条SQL语句</param>		
//        public static int ExecuteSqlTran(List<String> sqlList)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                Open(conn);
//                using (MySqlCommand cmd = new MySqlCommand())
//                {
//                    cmd.Connection = conn;
//                    using (MySqlTransaction trans = conn.BeginTransaction())
//                    {
//                        cmd.Transaction = trans;
//                        try
//                        {
//                            int count = 0;
//                            for (int n = 0; n < sqlList.Count; n++)
//                            {
//                                string strsql = sqlList[n];
//                                if (strsql.Trim().Length > 1)
//                                {
//                                    cmd.CommandText = strsql;
//                                    count += cmd.ExecuteNonQuery();
//                                }
//                            }
//                            trans.Commit();
//                            return count;
//                        }
//                        catch (MySqlException ex)
//                        {
//                            new LogHelper().WriteErrorLog(ex.Message);
//                            trans.Rollback();
//                            // throw new Common.Utilities.Exceptions.ExecuteSqlException
//                            // (e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/ExecuteSqlTran({0})", string.Join(";", sqlList.ToArray<string>())));
//                            return 0;
//                        }
//                        finally
//                        {
//                            Close(conn);
//                        }
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 执行多条SQL语句，实现数据库事务。
//        /// </summary>
//        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
//        public static int ExecuteSqlTran(System.Collections.Generic.List<CommandInfo> cmdList)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                conn.Open();
//                using (MySqlTransaction trans = conn.BeginTransaction())
//                {
//                    MySqlCommand cmd = new MySqlCommand();
//                    try
//                    {
//                        int count = 0;
//                        //循环
//                        foreach (CommandInfo myDE in cmdList)
//                        {
//                            string cmdText = myDE.CommandText;
//                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Parameters;
//                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);

//                            if (myDE.EffentNextType == EffentNextType.WhenHaveContine || myDE.EffentNextType == EffentNextType.WhenNoHaveContine)
//                            {
//                                if (myDE.CommandText.ToLower().IndexOf("count(") == -1)
//                                {
//                                    trans.Rollback();
//                                    return 0;
//                                }

//                                object obj = cmd.ExecuteScalar();
//                                bool isHave = false;
//                                if (obj == null && obj == DBNull.Value)
//                                {
//                                    isHave = false;
//                                }
//                                isHave = Convert.ToInt32(obj) > 0;

//                                if (myDE.EffentNextType == EffentNextType.WhenHaveContine && !isHave)
//                                {
//                                    trans.Rollback();
//                                    return 0;
//                                }
//                                if (myDE.EffentNextType == EffentNextType.WhenNoHaveContine && isHave)
//                                {
//                                    trans.Rollback();
//                                    return 0;
//                                }
//                                continue;
//                            }
//                            int val = cmd.ExecuteNonQuery();
//                            count += val;
//                            if (myDE.EffentNextType == EffentNextType.ExcuteEffectRows && val == 0)
//                            {
//                                trans.Rollback();
//                                return 0;
//                            }
//                            cmd.Parameters.Clear();
//                        }
//                        trans.Commit();
//                        return count;
//                    }
//                    catch(MySqlException ex)
//                    {
//                        new LogHelper().WriteErrorLog(ex.Message);
//                        trans.Rollback();
//                        throw;
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 查询第一行第一列的值
//        /// </summary>
//        /// <param name="SQLString">计算查询结果语句</param>
//        /// <returns></returns>
//        public static object GetSingle(string sql)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
//                {
//                    Open(conn);
//                    try
//                    {
//                        object obj = cmd.ExecuteScalar();
//                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
//                        {
//                            return null;
//                        }
//                        else
//                        {
//                            return obj;
//                        }
//                    }
//                    catch (MySqlException e)
//                    {
//                        //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/GetSingle({0})", sql));
//                        return null;
//                    }
//                    finally
//                    {
//                        Close(conn);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 查询第一行第一列的值
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="times"></param>
//        /// <returns></returns>
//        public static object GetSingle(string sql, int times)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
//                {
//                    Open(conn);
//                    try
//                    {
//                        cmd.CommandTimeout = times;
//                        object obj = cmd.ExecuteScalar();
//                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
//                        {
//                            return null;
//                        }
//                        else
//                        {
//                            return obj;
//                        }
//                    }
//                    catch (MySqlException e)
//                    {
//                        //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/GetSingle({0},{1})", sql, times));
//                        return null;
//                    }
//                    finally
//                    {
//                        Close(conn);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <returns></returns>
//        public static MySqlDataReader ExecuteReader(string sql)
//        {
//            MySqlConnection conn = new MySqlConnection(connectionString);
//            MySqlCommand cmd = new MySqlCommand(sql, conn);
//            Open(conn);
//            try
//            {
//                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
//                return myReader;
//            }
//            catch (MySqlException e)
//            {
//                //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/ExecuteReader({0})", sql));
//                return null;
//            }
//        }

//        /// <summary>
//        /// 执行查询语句，返回DataSet
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <returns></returns>
//        public static DataSet Query(string sql)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                DataSet ds = new DataSet();
//                Open(conn);
//                try
//                {
//                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn))
//                    {
//                        adapter.Fill(ds);
//                    }
//                }
//                catch (MySqlException e)
//                {
//                    //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/Query({0})", sql));
//                    return null;
//                }
//                finally
//                {
//                    Close(conn);
//                }
//                return ds;
//            }
//        }

//        /// <summary>
//        /// 执行查询语句，返回DataSet
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="times"></param>
//        /// <returns></returns>
//        public static DataSet Query(string sql, int times)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                DataSet ds = new DataSet();
//                Open(conn);
//                try
//                {
//                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn))
//                    {
//                        adapter.SelectCommand.CommandTimeout = times;
//                        adapter.Fill(ds);
//                    }
//                }
//                catch (MySqlException e)
//                {
//                    //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/Query({0},{1})", sql, times));
//                    return null;
//                }
//                finally
//                {
//                    Close(conn);
//                }
//                return ds;
//            }
//        }

//        #endregion

//        #region 执行带参数的SQL语句

//        /// <summary>
//        /// 执行SQL语句，返回影响的记录数
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <returns></returns>
//        public static int ExecuteSql(string sql, params MySqlParameter[] cmdParms)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                using (MySqlCommand cmd = new MySqlCommand())
//                {
//                    try
//                    {
//                        PrepareCommand(cmd, conn, null, sql, cmdParms);
//                        int rows = cmd.ExecuteNonQuery();
//                        cmd.Parameters.Clear();
//                        return rows;
//                    }
//                    catch (MySqlException e)
//                    {
//                        StringBuilder sBuilder = new StringBuilder();
//                        foreach (var p in cmdParms)
//                        {
//                            sBuilder.AppendFormat("{0};", p.Value);
//                        }
//                        if (sBuilder.Length > 0)
//                            sBuilder.Length -= 1;
//                        new LogHelper().WriteErrorLog(e.Message);
//                        return 0;
//                    }
//                    finally
//                    {
//                        Close(conn);
//                    }
//                }
//            }
//        }
//        /// <summary>
//        /// 执行多条SQL语句，实现数据库事务。
//        /// </summary>
//        /// <param name="sqlHash">SQL语句的哈希表(key为sql语句，value是该语句的MySqlParameter[])</param>
//        public static bool ExecuteSqlTran(Hashtable sqlHash)
//        {
//            bool flag = true;
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                Open(conn);
//                using (MySqlTransaction trans = conn.BeginTransaction())
//                {
//                    MySqlCommand cmd = new MySqlCommand();
//                    try
//                    {
//                        foreach (DictionaryEntry myDE in sqlHash)
//                        {
//                            string cmdText = myDE.Key.ToString();
//                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
//                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
//                            cmd.ExecuteNonQuery();
//                            cmd.Parameters.Clear();
//                        }
//                        trans.Commit();
//                    }
//                    catch (MySqlException e)
//                    {
//                        flag = false;
//                        trans.Rollback();
//                        StringBuilder sBuilder = new StringBuilder();
//                        foreach (DictionaryEntry myDE in sqlHash)
//                        {
//                            sBuilder.AppendFormat("key:{0},value:{1};", myDE.Key, myDE.Value);
//                        }
//                        if (sBuilder.Length > 0)
//                            sBuilder.Length -= 1;
//                        //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/ExecuteSqlTran(Hashtable[{0}])", sBuilder.ToString()));
//                        return false;
//                    }
//                    finally
//                    {
//                        Close(conn);
//                    }
//                }
//            }
//            return flag;
//        }
//        /// <summary>
//        /// 执行多条SQL语句，实现数据库事务。
//        /// </summary>
//        /// <param name="sqlHash">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
//        public static void ExecuteSqlTranWithIndentity(Hashtable sqlHash)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                Open(conn);
//                using (MySqlTransaction trans = conn.BeginTransaction())
//                {
//                    MySqlCommand cmd = new MySqlCommand();
//                    try
//                    {
//                        int indentity = 0;
//                        foreach (DictionaryEntry myDE in sqlHash)
//                        {
//                            string cmdText = myDE.Key.ToString();
//                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
//                            foreach (MySqlParameter q in cmdParms)
//                            {
//                                if (q.Direction == ParameterDirection.InputOutput)
//                                {
//                                    q.Value = indentity;
//                                }
//                            }
//                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
//                            int val = cmd.ExecuteNonQuery();
//                            foreach (MySqlParameter q in cmdParms)
//                            {
//                                if (q.Direction == ParameterDirection.Output)
//                                {
//                                    indentity = Convert.ToInt32(q.Value);
//                                }
//                            }
//                            cmd.Parameters.Clear();
//                        }
//                        trans.Commit();
//                    }
//                    catch (MySqlException e)
//                    {
//                        trans.Rollback();
//                        StringBuilder sBuilder = new StringBuilder();
//                        foreach (DictionaryEntry myDE in sqlHash)
//                        {
//                            sBuilder.AppendFormat("key:{0},value:{1};", myDE.Key, myDE.Value);
//                        }
//                        if (sBuilder.Length > 0)
//                            sBuilder.Length -= 1;
//                        //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/ExecuteSqlTranWithIndentity(Hashtable[{0}])", sBuilder.ToString()));

//                    }
//                    finally
//                    {
//                        Close(conn);
//                    }
//                }
//            }
//        }
//        /// <summary>
//        /// 查询第一行第一列的值
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="cmdParms"></param>
//        /// <returns></returns>
//        public static object GetSingle(string sql, params MySqlParameter[] cmdParms)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                using (MySqlCommand cmd = new MySqlCommand())
//                {
//                    try
//                    {
//                        PrepareCommand(cmd, conn, null, sql, cmdParms);
//                        object obj = cmd.ExecuteScalar();
//                        cmd.Parameters.Clear();
//                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
//                        {
//                            return null;
//                        }
//                        else
//                        {
//                            return obj;
//                        }
//                    }
//                    catch (MySqlException e)
//                    {
//                        StringBuilder sBuilder = new StringBuilder();
//                        foreach (var p in cmdParms)
//                        {
//                            sBuilder.AppendFormat("{0};", p.Value);
//                        }
//                        if (sBuilder.Length > 0)
//                            sBuilder.Length -= 1;
//                        //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/GetSingle({0},{1})", sql, sBuilder.ToString()));
//                        return null;
//                    }
//                    finally
//                    {
//                        Close(conn); ;
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="cmdParms"></param>
//        /// <returns></returns>
//        public static MySqlDataReader ExecuteReader(string sql, params MySqlParameter[] cmdParms)
//        {
//            MySqlConnection conn = new MySqlConnection(connectionString);
//            MySqlCommand cmd = new MySqlCommand();
//            try
//            {
//                PrepareCommand(cmd, conn, null, sql, cmdParms);
//                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
//                cmd.Parameters.Clear();
//                return myReader;
//            }
//            catch (MySqlException e)
//            {
//                StringBuilder sBuilder = new StringBuilder();
//                foreach (var p in cmdParms)
//                {
//                    sBuilder.AppendFormat("{0};", p.Value);
//                }
//                if (sBuilder.Length > 0)
//                    sBuilder.Length -= 1;
//                new LogHelper().WriteErrorLog(e.Message);
//                //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/ExecuteReader({0},{1})", sql, sBuilder.ToString()));
//                return null;
//            }
//        }

//        /// <summary>
//        /// 自定义分页(MySqlDataReader)--AddBY---wpw 
//        /// </summary>
//        /// <param name="pageSize">页数</param>
//        /// <param name="currentPage">当前页</param>
//        /// <param name="paras">参数数组 0-列 1-列2 2-表名 3-条件 4-排序字段</param>
//        /// <returns>列表</returns>
//        public static MySqlDataReader GetPageList(int currentPage, int pageSize, params string[] paras)
//        {
//            string template = "select {0} from {1} where {2} order by {3} limit {4},{5}";
//            if (string.IsNullOrEmpty(paras[3]))
//            {
//                template = "select {0} from {1}{2} order by {3}  limit {4},{5}";
//            }
//            string strSql = string.Format(template
//                                          , paras[0]
//                                          , paras[2]
//                                          , paras[3]
//                                          , paras[4]
//                                          , pageSize * (currentPage - 1)
//                                          , pageSize);
//            return MysqlHelper.ExecuteReader(strSql);
//        }

//        /// <summary>
//        /// 自定义分页 参数化(MySqlDataReader)--AddBY---wpw 
//        /// </summary>
//        /// <param name="pageSize">页数</param>
//        /// <param name="currentPage">当前页</param>
//        /// <param name="pram">参数列表</param>
//        /// <param name="paras">参数数组 0-列  1-表名 2-条件 3-排序字段</param>
//        /// <returns>列表</returns>
//        public static MySqlDataReader GetPageListParams(int currentPage, int pageSize, MySqlParameter[] pram, params string[] paras)
//        {
//            string template = "select {0} from {1} where {2} order by {3} limit {4},{5}";
//            string strSql = string.Format(template
//                                          , paras[0]
//                                          , paras[1]
//                                          , paras[2]
//                                          , paras[3]
//                                          , pageSize * (currentPage - 1)
//                                          , pageSize);
//            if (paras[3] == "" || paras[3] == null)
//            {
//                template = "select {0} from {1} where {2}  limit {3},{4}";
//                strSql = string.Format(template
//                                          , paras[0]
//                                          , paras[1]
//                                          , paras[2]
//                                          , pageSize * (currentPage - 1)
//                                          , pageSize);
//            }
//            //string template = "select {0} from {1} where {2}  limit {3},{4}";

//            return MysqlHelper.ExecuteReader(strSql, pram);
//        }

//        /// <summary>
//        /// 分页 带 总数
//        /// </summary>
//        /// <param name="pageSize">页数</param>
//        /// <param name="currentPage">当前页</param>
//        /// <param name="count"></param>
//        /// <param name="paras">参数数组 0-列 1-列2 2-表名 3-条件 4-排序字段</param>
//        /// <returns>列表</returns>
//        public static MySqlDataReader GetPageListOutNum(int currentPage, int pageSize, out int count, params string[] paras)
//        {
//            string template = "select {0} from {1} where {2} order by {3} limit {4},{5}";
//            if (string.IsNullOrEmpty(paras[4]))
//            {
//                template = "select {0} from {1}{2} order by {3}  limit {4},{5}";
//            }
//            string strSql = string.Format(template
//                                          , paras[0]
//                                          , paras[2]
//                                          , paras[3]
//                                          , paras[4]
//                                          , pageSize * (currentPage - 1)
//                                          , pageSize);

//            string sqlCount = string.Format("select count(1) from {0} where {1}", paras[2], paras[3]);
//            count = Convert.ToInt32(MysqlHelper.GetSingle(sqlCount));
//            return MysqlHelper.ExecuteReader(strSql);
//        }
//        /// <summary>
//        /// 分页 带 总数
//        /// </summary>
//        /// <param name="currentPage"></param>
//        /// <param name="pageSize"></param>
//        /// <param name="count"></param>
//        /// <param name="paras"></param>
//        /// <param name="cmdParms"></param>
//        /// <returns></returns>
//        public static MySqlDataReader GetPageListOutNum(int currentPage, int pageSize, out int count, string[] paras, params MySqlParameter[] cmdParms)
//        {
//            string template = "select {0} from {1} where {2} order by {3} limit {4},{5}";
//            if (string.IsNullOrEmpty(paras[4]))
//            {
//                template = "select {0} from {1}{2} order by {3}  limit {4},{5}";
//            }
//            string strSql = string.Format(template
//                                          , paras[0]
//                                          , paras[2]
//                                          , paras[3]
//                                          , paras[4]
//                                          , pageSize * (currentPage - 1)
//                                          , pageSize);

//            string sqlCount = string.Format("select count(1) from {0} where {1}", paras[2], paras[3]);
//            count = Convert.ToInt32(MysqlHelper.GetSingle(sqlCount, cmdParms));
//            return MysqlHelper.ExecuteReader(strSql, cmdParms);
//        }
//        /// <summary>
//        /// 获取应该的 数量,并不  排序
//        /// </summary>
//        /// <param name="num"></param>
//        /// <param name="paras"></param>
//        /// <returns></returns>
//        public static MySqlDataReader GetListSome(int num, params string[] paras)
//        {

//            string template = "select {0} from {1} where {2}  limit {4},{5}";
//            if (string.IsNullOrEmpty(paras[4]))
//            {
//                template = "select {0} from {1}{2}   limit {4},{5}";
//            }
//            string strSql = string.Format(template
//                                          , paras[0]
//                                          , paras[2]
//                                          , paras[3]
//                                          , paras[4]);


//            return MysqlHelper.ExecuteReader(strSql);


//        }


//        /// <summary>
//        /// 执行查询语句，返回DataSet
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="cmdParms"></param>
//        /// <returns></returns>
//        public static DataSet Query(string sql, params MySqlParameter[] cmdParms)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                MySqlCommand cmd = new MySqlCommand();
//                PrepareCommand(cmd, conn, null, sql, cmdParms);
//                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
//                {
//                    DataSet ds = new DataSet();
//                    try
//                    {
//                        da.Fill(ds);
//                        cmd.Parameters.Clear();
//                    }
//                    catch (MySqlException e)
//                    {
//                        StringBuilder sBuilder = new StringBuilder();
//                        foreach (var p in cmdParms)
//                        {
//                            sBuilder.AppendFormat("{0};", p.Value);
//                        }
//                        if (sBuilder.Length > 0)
//                            sBuilder.Length -= 1;
//                        // throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/Query({0},{1})", sql, sBuilder.ToString()));
//                    }
//                    finally
//                    {
//                        Close(conn); ;
//                    }
//                    return ds;
//                }
//            }
//        }

//        /// <summary>
//        /// 组装MySqlCommand
//        /// </summary>
//        /// <param name="cmd"></param>
//        /// <param name="conn"></param>
//        /// <param name="trans"></param>
//        /// <param name="cmdText"></param>
//        /// <param name="cmdParms"></param>
//        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
//        {
//            Open(conn);
//            cmd.Connection = conn;
//            cmd.CommandText = cmdText;
//            if (trans != null)
//                cmd.Transaction = trans;
//            cmd.CommandType = CommandType.Text;//cmdType;
//            if (cmdParms != null)
//            {
//                foreach (MySqlParameter parameter in cmdParms)
//                {
//                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
//                        (parameter.Value == null))
//                    {
//                        parameter.Value = DBNull.Value;
//                    }
//                    cmd.Parameters.Add(parameter);
//                }
//            }
//        }

//        #endregion

//        #region 存储过程操作

//        /// <summary>
//        /// 执行存储过程，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
//        /// </summary>
//        /// <param name="storedProcName">存储过程名</param>
//        /// <param name="parameters">存储过程参数</param>
//        /// <returns></returns>
//        public static MySqlDataReader RunProcedureDataReader(string storedProcName, params IDataParameter[] parameters)
//        {
//            MySqlConnection conn = new MySqlConnection(connectionString);
//            Open(conn);
//            try
//            {
//                MySqlCommand command = BuildQueryCommand(conn, storedProcName, parameters);
//                command.CommandType = CommandType.StoredProcedure;
//                return command.ExecuteReader(CommandBehavior.CloseConnection);
//            }
//            catch (MySqlException e)
//            {
//                StringBuilder sBuilder = new StringBuilder();
//                foreach (var p in parameters)
//                {
//                    sBuilder.AppendFormat("{0};", p.Value);
//                }
//                if (sBuilder.Length > 0)
//                    sBuilder.Length -= 1;
//                //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/RunProcedure({0},{1}),MySqlDataReader", storedProcName, sBuilder.ToString()));
//                return null;

//            }
//        }

//        /// <summary>
//        /// 执行存储过程
//        /// </summary>
//        /// <param name="storedProcName">存储过程名</param>
//        /// <param name="parameters">存储过程参数</param>
//        /// <param name="tableName">DataSet结果中的表名</param>
//        /// <returns></returns>
//        public static DataSet RunProcedure(string storedProcName, params IDataParameter[] parameters)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                DataSet ds = new DataSet();
//                Open(conn);

//                try
//                {
//                    using (MySqlDataAdapter sqlDA = new MySqlDataAdapter())
//                    {

//                        sqlDA.SelectCommand = BuildQueryCommand(conn, storedProcName, parameters);
//                        sqlDA.Fill(ds);
//                    }
//                }
//                catch (MySqlException e)
//                {
//                    StringBuilder sBuilder = new StringBuilder();
//                    foreach (var p in parameters)
//                    {
//                        sBuilder.AppendFormat("{0};", p.Value);
//                    }
//                    if (sBuilder.Length > 0)
//                        sBuilder.Length -= 1;
//                    //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/RunProcedure({0},{1}),DataSet", storedProcName, sBuilder.ToString()));
//                    return null;

//                }
//                finally
//                {
//                    Close(conn);
//                }
//                return ds;
//            }
//        }
//        /// <summary>
//        /// 执行存储过程
//        /// </summary>
//        /// <param name="storedProcName"></param>
//        /// <param name="parameters"></param>
//        /// <param name="Times"></param>
//        /// <returns></returns>
//        public static DataSet RunProcedure(string storedProcName, int Times, params IDataParameter[] parameters)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                DataSet ds = new DataSet();
//                Open(conn);

//                try
//                {
//                    using (MySqlDataAdapter sqlDA = new MySqlDataAdapter())
//                    {
//                        sqlDA.SelectCommand = BuildQueryCommand(conn, storedProcName, parameters);
//                        sqlDA.SelectCommand.CommandTimeout = Times;
//                        sqlDA.Fill(ds);
//                    }
//                }
//                catch (MySqlException e)
//                {
//                    StringBuilder sBuilder = new StringBuilder();
//                    foreach (var p in parameters)
//                    {
//                        sBuilder.AppendFormat("{0};", p.Value);
//                    }
//                    if (sBuilder.Length > 0)
//                        sBuilder.Length -= 1;
//                    // throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/RunProcedure({0},{1},{2}),DataSet", storedProcName, sBuilder.ToString(), Times));
//                    return null;
//                }
//                finally
//                {
//                    Close(conn);
//                }
//                return ds;
//            }
//        }

//        /// <summary>
//        /// 构建 MySqlCommand 对象(用来返回一个结果集，而不是一个整数值)
//        /// </summary>
//        /// <param name="conn">数据库连接</param>
//        /// <param name="storedProcName">存储过程名</param>
//        /// <param name="parameters">存储过程参数</param>
//        /// <returns>MySqlCommand</returns>
//        private static MySqlCommand BuildQueryCommand(MySqlConnection conn, string storedProcName, IDataParameter[] parameters)
//        {
//            MySqlCommand command = new MySqlCommand(storedProcName, conn);
//            command.CommandType = CommandType.StoredProcedure;

//            foreach (MySqlParameter parameter in parameters)
//            {
//                if (parameter != null)
//                {
//                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
//                        (parameter.Value == null))
//                    {
//                        parameter.Value = DBNull.Value;
//                    }
//                    command.Parameters.Add(parameter);
//                }
//            }

//            return command;
//        }

//        /// <summary>
//        /// 执行存储过程，返回影响的行数		
//        /// </summary>
//        /// <param name="storedProcName">存储过程名</param>
//        /// <param name="parameters">存储过程参数</param>
//        /// <param name="rowsAffected">影响的行数</param>
//        /// <returns></returns>
//        public static int RunProcedure(string storedProcName, out int rowsAffected, params IDataParameter[] parameters)
//        {
//            using (MySqlConnection conn = new MySqlConnection(connectionString))
//            {
//                Open(conn);
//                try
//                {
//                    MySqlCommand command = BuildIntCommand(conn, storedProcName, parameters);
//                    rowsAffected = command.ExecuteNonQuery();
//                    return (int)command.Parameters["ReturnValue"].Value;
//                }
//                catch (MySqlException e)
//                {
//                    StringBuilder sBuilder = new StringBuilder();
//                    foreach (var p in parameters)
//                    {
//                        sBuilder.AppendFormat("{0};", p.Value);
//                    }
//                    if (sBuilder.Length > 0)
//                        sBuilder.Length -= 1;
//                    rowsAffected = 0;
//                    //throw new Common.Utilities.Exceptions.ExecuteSqlException(e.Message, string.Format("ZY.Infrastructure.DbUtilities/MySqlHelper/RunProcedure({0},{1},out int rowsAffected)", storedProcName, sBuilder.ToString()));
//                    return 0;
//                }
//                finally
//                {
//                    Close(conn);
//                }
//            }
//        }

//        /// <summary>
//        /// 创建 MySqlCommand 对象实例
//        /// </summary>
//        /// <param name="storedProcName">存储过程名</param>
//        /// <param name="parameters">存储过程参数</param>
//        /// <returns>MySqlCommand 对象实例</returns>
//        private static MySqlCommand BuildIntCommand(MySqlConnection conn, string storedProcName, IDataParameter[] parameters)
//        {
//            MySqlCommand command = BuildQueryCommand(conn, storedProcName, parameters);
//            command.Parameters.Add(new MySqlParameter("ReturnValue",
//                MySqlDbType.Int32, 4, ParameterDirection.ReturnValue,
//                false, 0, 0, string.Empty, DataRowVersion.Default, null));
//            return command;
//        }
//        #endregion
//        #region 转化为 MySqlParameter
//        public static MySqlParameter[] ToMysqlParament(Dictionary<string, object> dic)
//        {
//            if (dic == null)
//                return null;
//            var i = 0;
//            var paras = new MySqlParameter[dic.Count];
//            foreach (var item in dic)
//            {
//                paras[i] = new MySqlParameter(item.Key, item.Value);
//                i++;
//            }
//            return paras;

//        }
//        #endregion
//    }
//}
