namespace DbOperateHelper.ConsoleApp.ServerTest.Entity
{
    public class FaceInfoRecord
    {
        public int UserId { get; set; }
        public string SerialNumber { get; set; }
        public int Status { get; set; }
        public byte[] Image { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Notes { get; set; }
        public int RecordId { get; set; }
        public int VersionId { get; set; }
        public string UserName { get; set; }
    }
}
