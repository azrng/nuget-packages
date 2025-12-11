using System;
using System.Collections.Generic;
using System.Data;

namespace CommonCollect.DbConnection
{
    /// <summary>
    ///接口定义  还不完善还应该支持一些操作
    /// </summary>
    public interface ISqlConnectionHelper
    {
        void Execute(string sql);

        void Execute(string sql, object[] sqlParams);

        T Query<T>(string sql, Func<IDataReader, T> func, params object[] sqlParams);

        List<T> Query<T>(string sql, params object[] sqlParams);
    }
}