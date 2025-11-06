using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace OnewheroVisitorManagement.Models
{
    public class Visitor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        public string LastName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("interests")]
        public List<string> Interests { get; set; }

        [BsonElement("registrationDate")]
        public DateTime RegistrationDate { get; set; }

        [BsonElement("visitCount")]
        public int VisitCount { get; set; }

        public Visitor()
        {
            Interests = new List<string>();
            RegistrationDate = DateTime.Now;
            VisitCount = 0;
        }
    }
}