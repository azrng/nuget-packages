namespace CommonCollect.GenDbSqlBridge.Model
{
    public class PrimaryModel
    {
        public string TableName { get; set; }
        public string ColName { get; set; }
        public int ColSort { get; set; }

        public string ColConstraintName { get; set; }
    }
}
