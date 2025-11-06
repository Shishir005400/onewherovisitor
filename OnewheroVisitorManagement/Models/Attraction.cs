using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnewheroVisitorManagement.Models
{
    public class Attraction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("visitCount")]
        public int VisitCount { get; set; }

        [BsonElement("isOpen")]
        public bool IsOpen { get; set; }
    }
}