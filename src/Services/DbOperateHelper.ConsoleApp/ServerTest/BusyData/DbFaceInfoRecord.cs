using Azrng.DbOperateHelper;
using DbOperateHelper.ConsoleApp.ServerTest.Entity;
using System.Data;
using System.Data.Common;

namespace DbOperateHelper.ConsoleApp.ServerTest.BusyData
{
    public class DbFaceInfoRecord : Common
    {
        public DbFaceInfoRecord() : base(DataBaseFactory.Db, "faceinforecord")
        {

        }
        private FaceInfoRecord RowToModel(IDataReader row)
        {
            FaceInfoRecord entity = new FaceInfoRecord
            {
                UserId = row.GetInt32Ex(row.GetOrdinal("UserId")),
                SerialNumber = row.GetStringEx(row.GetOrdinal("SerialNumber")),
                Status = row.GetInt32Ex(row.GetOrdinal("Status")),
                Notes = row.GetStringEx(row.GetOrdinal("Notes")),
                CreationDate = row.GetDateTimeEx(row.GetOrdinal("CreationDate")),
                UpdateDate = row.GetDateTimeEx(row.GetOrdinal("UpdateDate")),
                Image = row.GetBytesEx(row.GetOrdinal("Image")),
                VersionId = row.GetInt32Ex(row.GetOrdinal("VersionId")),
                RecordId = row.GetInt32Ex(row.GetOrdinal("RecordId")),
                UserName = row.GetStringEx(row.GetOrdinal("UserName")),
            };

            return entity;
        }
        /// <summary>
        /// 查询单条long型数据
        /// </summary>
        /// <param name="row">数据游标</param>
        /// <returns></returns>
        public static long DataReaderForLong(IDataReader row)
        {
            return DbDataReaderEx.GetInt64Ex(row, 0);
        }

        /// <summary>
        /// 查询单条int型数据
        /// </summary>
        /// <param name="row">数据游标</param>
        /// <returns></returns>
        public static int DataReaderForInt(IDataReader row)
        {
            return DbDataReaderEx.GetInt32Ex(row, 0);
        }
        public int GetCount()
        {
            string sql = string.Format("SELECT COUNT(*) FROM {0}", TableName);

            return QueryFirstOrDefault(sql, DataReaderForInt);
        }
        public bool Insert(FaceInfoRecord faceInfoRecord)
        {
            string sql = string.Format("INSERT INTO {0} (VersionId,UserId,SerialNumber,Status,Notes,CreationDate,UpdateDate,Image) " +
                "VALUES (@VersionId,@UserId,@SerialNumber,@Status,@Notes,@CreationDate,@UpdateDate,@Image)"
            , TableName);

            DbParameter[] parameters = {
                CreateDbParameter("@VersionId", DbType.Int32,faceInfoRecord.VersionId),
                CreateDbParameter("@UserId", DbType.Int32,faceInfoRecord.UserId),
                CreateDbParameter("@SerialNumber", DbType.String,faceInfoRecord.UserId),
                CreateDbParameter("@Status", DbType.Int32,faceInfoRecord.Status),
                CreateDbParameter("@Notes", DbType.String,faceInfoRecord.Notes),
                CreateDbParameter("@CreationDate", DbType.DateTime,faceInfoRecord.CreationDate),
                CreateDbParameter("@UpdateDate", DbType.DateTime,faceInfoRecord.UpdateDate),
                CreateDbParameter("@Image", DbType.Binary,faceInfoRecord.Image)
            };

            return Execute(sql, parameters) > 0;
        }
        public List<FaceInfoRecord> GetListInfo()
        {
            string sql = string.Format("SELECT * FROM {0}", TableName);

            return Query(sql, RowToModel);
        }
        public DataTable GetDataTableInfo()
        {
            string sql = string.Format("SELECT * FROM {0}", TableName);

            return Query(sql).Tables[0];
        }
        public FaceInfoRecord GetInfo()
        {
            string sql = string.Format("SELECT * FROM {0} Limit 1", TableName);

            return QueryFirstOrDefault(sql, RowToModel);
        }
        public FaceInfoRecord GetInfoWithSerialNumber(string serialNumber)
        {
            string sql = string.Format("SELECT * FROM {0} WHERE SerialNumber=@serialNumber", TableName);

            return QueryFirstOrDefault(sql, RowToModel, CreateDbParameter("@SerialNumber", DbType.String, serialNumber));
        }

    }
}
