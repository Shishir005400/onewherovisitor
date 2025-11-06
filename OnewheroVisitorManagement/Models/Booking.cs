using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OnewheroVisitorManagement.Models
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("visitorId")]
        public string VisitorId { get; set; }

        [BsonElement("eventId")]
        public string EventId { get; set; }

        [BsonElement("bookingDate")]
        public DateTime BookingDate { get; set; }

        [BsonElement("numberOfTickets")]
        public int NumberOfTickets { get; set; }

        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        public Booking()
        {
            BookingDate = DateTime.Now;
            Status = "Confirmed";
        }
    }
}