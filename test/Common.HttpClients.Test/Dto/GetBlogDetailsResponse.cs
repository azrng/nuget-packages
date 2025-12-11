namespace Common.HttpClients.Test.Dto
{
    public class GetBlogDetailsResponse
    {
        public int userId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }

    public class AddBlogRequest
    {
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }
}