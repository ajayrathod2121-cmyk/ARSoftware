using SQLite;

namespace ARSoftware.Models
{
    public class UserModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Username { get; set; }

        public string Password { get; set; }

        [Unique]
        public string Mobile { get; set; }

        public string BusinessName { get; set; }
    }
}