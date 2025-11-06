using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OnewheroVisitorManagement.Models
{
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("eventName")]
        public string EventName { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("eventDate")]
        public DateTime EventDate { get; set; }

        [BsonElement("eventType")]
        public string EventType { get; set; } 

        [BsonElement("capacity")]
        public int Capacity { get; set; }

        [BsonElement("availableSeats")]
        public int AvailableSeats { get; set; }

        [BsonElement("ticketPrice")]
        public decimal TicketPrice { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}