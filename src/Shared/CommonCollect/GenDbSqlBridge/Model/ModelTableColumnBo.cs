namespace CommonCollect.GenDbSqlBridge.Model
{
    public class ModelTableColumnBo
    {
        public int Id { get; set; }

        public int ModelId { get; set; }

        public string ColumnName { get; set; }

        public string ColumnCnName { get; set; }

        public string ColumnType { get; set; }

        public string ColumnLength { get; set; }

        public bool IsNull { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsIdentity { get; set; }

        /// <summary>
        ///  数据元id
        /// </summary>
        public int DataElementId { get; set; }

        public string Comment { get; set; }

        public int StructId { get; set; }

        public string CalcMode { get; set; }

        public string DefaultValue { get; set; }

        /// <summary>
        /// 列排序号
        /// </summary>
        public int? RowNumber { get; set; }

        /// <summary>
        ///     创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        ///     创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        ///     修改人
        /// </summary>
        public string UpdateUser { get; set; }

        /// <summary>
        ///     更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        public int CreateUserId { get; set; }

        public int? UpdateUserId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj as ModelTableColumnBo == null) return false;
            var obj2 = obj as ModelTableColumnBo;
            return obj2.Id == Id &&
                   obj2.ModelId == ModelId &&
                   obj2.ColumnName == ColumnName &&
                   obj2.ColumnCnName == ColumnCnName &&
                   obj2.ColumnType == ColumnType &&
                   obj2.ColumnLength == ColumnLength &&
                   obj2.ColumnName == ColumnName &&
                   obj2.IsNull == IsNull &&
                   obj2.IsPrimaryKey == IsPrimaryKey &&
                   obj2.IsForeignKey == IsForeignKey &&
                   obj2.IsIdentity == IsIdentity &&
                   obj2.DataElementId == DataElementId &&
                   obj2.Comment == Comment &&
                   obj2.StructId == StructId &&
                   obj2.CalcMode == CalcMode &&
                   obj2.DefaultValue == DefaultValue &&
                   obj2.RowNumber == RowNumber;
        }

        public bool StructEquals(ModelTableColumnBo obj2)
        {
            return obj2.ColumnName == ColumnName &&
                   obj2.ColumnCnName == ColumnCnName &&
                   obj2.ColumnType == ColumnType &&
                   obj2.ColumnLength == ColumnLength &&
                   obj2.ColumnName == ColumnName &&
                   obj2.IsNull == IsNull &&
                   obj2.IsPrimaryKey == IsPrimaryKey &&
                   obj2.IsForeignKey == IsForeignKey &&
                   obj2.IsIdentity == IsIdentity &&
                   obj2.DataElementId == DataElementId &&
                   obj2.Comment == Comment &&
                   obj2.CalcMode == CalcMode &&
                   obj2.DefaultValue == DefaultValue &&
                   obj2.RowNumber == RowNumber;
        }
    }
}