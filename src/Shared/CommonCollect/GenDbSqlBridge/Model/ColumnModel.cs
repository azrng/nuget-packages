namespace CommonCollect.GenDbSqlBridge.Model
{
    public class ColumnModel
    {
        public string? TableName { get; set; }
        public string ColName { get; set; }
        public string ColLength { get; set; }
        public string ColType { get; set; }
        public string ColDefault { get; set; }
        public string ColComment { get; set; }
        public bool Is_Null { get; set; }
        public bool Is_Identity { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsForeignKey { get; set; }
        public string StructColType { get; set; }
        public string StructColLength { get; set; }
        public string StructDefaultValue { get; set; }
        public int RowNumber { get; set; }
    }
}
