using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OnewheroVisitorManagement.Models
{
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("eventName")]
        public string EventName { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("eventType")]
        public string EventType { get; set; } = string.Empty;

        [BsonElement("eventDate")]
        public DateTime EventDate { get; set; }

        [BsonElement("capacity")]
        public int Capacity { get; set; }

        [BsonElement("availableSeats")]
        public int AvailableSeats { get; set; }

        [BsonElement("ticketPrice")]
        public decimal TicketPrice { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        public string FormattedDate => EventDate.ToString("dd/MM/yyyy");
    }
}