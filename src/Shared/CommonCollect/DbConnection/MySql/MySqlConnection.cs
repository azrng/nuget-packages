using CommonCollect.DbConnection.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;

namespace CommonCollect.DbConnection.MySql
{
    public class MySqlConnectionHelper : ISqlConnectionHelper
    {
        private readonly MySqlOptions _mySqlOption;

        public MySqlConnectionHelper(IOptions<MySqlOptions> options)
        {
            _mySqlOption = options?.Value;
        }

        public void Execute(string sql)
        {
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0011: Expected O, but got Unknown
            var val = new MySqlConnector.MySqlConnection(_mySqlOption.ConnectionString);
            try
            {
                ((IDbConnection)val).ExecuteNonQuery(sql, null);
            }
            finally
            {
                ((IDisposable)val)?.Dispose();
            }
        }

        public void Execute(string sql, object[] sqlParams)
        {
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0011: Expected O, but got Unknown
            var val = new MySqlConnector.MySqlConnection(_mySqlOption.ConnectionString);
            try
            {
                ((IDbConnection)val).ExecuteNonQuery(sql, null, sqlParams);
            }
            finally
            {
                ((IDisposable)val)?.Dispose();
            }
        }

        public T Query<T>(string sql, Func<IDataReader, T> func, params object[] sqlParams)
        {
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0011: Expected O, but got Unknown
            var val = new MySqlConnector.MySqlConnection(_mySqlOption.ConnectionString);
            try
            {
                return val.ExecuteReader(sql, func, sqlParams);
            }
            finally
            {
                ((IDisposable)val)?.Dispose();
            }
        }

        public List<T> Query<T>(string sql, params object[] sqlParams)
        {
            //IL_000b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0011: Expected O, but got Unknown
            var val = new MySqlConnector.MySqlConnection(_mySqlOption.ConnectionString);
            try
            {
                return ((IDbConnection)val).ExecuteReader<T>(sql, sqlParams);
            }
            finally
            {
                ((IDisposable)val)?.Dispose();
            }
        }
    }
}
