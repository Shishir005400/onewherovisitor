using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OnewheroVisitorManagement.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("fullName")]
        public string FullName { get; set; }

        [BsonElement("role")]
        public string Role { get; set; } // "Admin" or "Staff"

        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        public User()
        {
            CreatedDate = DateTime.Now;
            IsActive = true;
        }
    }
}