namespace Azrng.Core.DefaultJson.Test.Models
{
    public class UserInfo
    {
        public int UserId { get; set; }

        public string FirstName { get; set; }

        public double Salary { get; set; }

        public bool IsAdmin { get; set; }

        public List<string> Roles { get; set; }

        public long CreatedAt { get; set; }
    }
}