using MongoDB.Bson;

namespace backend.Models
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }

    }
}
