namespace CommonCollect.GenDbSqlBridge.Model
{
    public class TableModel
    {
        public string TableName { get; set; }
        public string TableComment { get; set; }
        public List<ColumnModel> Columns { get; set; } = new List<ColumnModel>();

        public List<PrimaryModel> Primarys { get; set; } = new List<PrimaryModel>();
    }
}
