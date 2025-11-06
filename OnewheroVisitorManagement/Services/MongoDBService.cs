using MongoDB.Driver;
using OnewheroVisitorManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnewheroVisitorManagement.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Visitor> _visitors;
        private readonly IMongoCollection<Event> _events;
        private readonly IMongoCollection<Booking> _bookings;
        private readonly IMongoCollection<Attraction> _attractions;

        public MongoDBService()
        {
            // ⚠️ IMPORTANT: Replace with YOUR connection string from MongoDB Atlas
            string connectionString = "mongodb+srv://onewhero_admin:Shishir.Kandel1@cluster0.3lvwypx.mongodb.net/?appName=Cluster0";

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("OnewheroHeritagePark");

            _visitors = _database.GetCollection<Visitor>("Visitors");
            _events = _database.GetCollection<Event>("Events");
            _bookings = _database.GetCollection<Booking>("Bookings");
            _attractions = _database.GetCollection<Attraction>("Attractions");
        }

        // ==================== VISITOR OPERATIONS ====================

        public async Task<string> AddVisitorAsync(Visitor visitor)
        {
            await _visitors.InsertOneAsync(visitor);
            return visitor.Id;
        }

        public async Task<List<Visitor>> GetAllVisitorsAsync()
        {
            return await _visitors.Find(_ => true).ToListAsync();
        }

        public async Task<Visitor> GetVisitorByIdAsync(string id)
        {
            return await _visitors.Find(v => v.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Visitor> GetVisitorByEmailAsync(string email)
        {
            return await _visitors.Find(v => v.Email == email).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateVisitorAsync(string id, Visitor visitor)
        {
            var result = await _visitors.ReplaceOneAsync(v => v.Id == id, visitor);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteVisitorAsync(string id)
        {
            var result = await _visitors.DeleteOneAsync(v => v.Id == id);
            return result.DeletedCount > 0;
        }

        // ==================== EVENT OPERATIONS ====================

        public async Task<string> AddEventAsync(Event evt)
        {
            await _events.InsertOneAsync(evt);
            return evt.Id;
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _events.Find(_ => true).ToListAsync();
        }

        public async Task<List<Event>> GetActiveEventsAsync()
        {
            return await _events.Find(e => e.IsActive && e.EventDate > DateTime.Now).ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(string id)
        {
            return await _events.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateEventAsync(string id, Event evt)
        {
            var result = await _events.ReplaceOneAsync(e => e.Id == id, evt);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteEventAsync(string id)
        {
            var result = await _events.DeleteOneAsync(e => e.Id == id);
            return result.DeletedCount > 0;
        }

        // ==================== BOOKING OPERATIONS ====================

        public async Task<string> CreateBookingAsync(Booking booking)
        {
            // Check if seats are available
            var evt = await GetEventByIdAsync(booking.EventId);
            if (evt != null && evt.AvailableSeats >= booking.NumberOfTickets)
            {
                // Update available seats
                evt.AvailableSeats -= booking.NumberOfTickets;
                await UpdateEventAsync(evt.Id, evt);

                // Create booking
                await _bookings.InsertOneAsync(booking);

                // Update visitor visit count
                var visitor = await GetVisitorByIdAsync(booking.VisitorId);
                if (visitor != null)
                {
                    visitor.VisitCount++;
                    await UpdateVisitorAsync(visitor.Id, visitor);
                }

                return booking.Id;
            }
            return null;
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _bookings.Find(_ => true).ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByVisitorAsync(string visitorId)
        {
            return await _bookings.Find(b => b.VisitorId == visitorId).ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByEventAsync(string eventId)
        {
            return await _bookings.Find(b => b.EventId == eventId).ToListAsync();
        }

        public async Task<bool> CancelBookingAsync(string bookingId)
        {
            var booking = await _bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
            if (booking != null)
            {
                booking.Status = "Cancelled";

                // Return seats to event
                var evt = await GetEventByIdAsync(booking.EventId);
                if (evt != null)
                {
                    evt.AvailableSeats += booking.NumberOfTickets;
                    await UpdateEventAsync(evt.Id, evt);
                }

                var result = await _bookings.ReplaceOneAsync(b => b.Id == bookingId, booking);
                return result.ModifiedCount > 0;
            }
            return false;
        }

        // ==================== ATTRACTION OPERATIONS ====================

        public async Task<string> AddAttractionAsync(Attraction attraction)
        {
            await _attractions.InsertOneAsync(attraction);
            return attraction.Id;
        }

        public async Task<List<Attraction>> GetAllAttractionsAsync()
        {
            return await _attractions.Find(_ => true).ToListAsync();
        }

        public async Task<Attraction> GetAttractionByIdAsync(string id)
        {
            return await _attractions.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        // ==================== ANALYTICS ====================

        public async Task<Dictionary<string, int>> GetVisitorDemographicsAsync()
        {
            var visitors = await GetAllVisitorsAsync();
            var demographics = new Dictionary<string, int>();

            foreach (var visitor in visitors)
            {
                foreach (var interest in visitor.Interests)
                {
                    if (demographics.ContainsKey(interest))
                        demographics[interest]++;
                    else
                        demographics[interest] = 1;
                }
            }

            return demographics;
        }

        public async Task<int> GetTotalVisitorCountAsync()
        {
            return (int)await _visitors.CountDocumentsAsync(_ => true);
        }

        public async Task<int> GetTotalBookingsCountAsync()
        {
            return (int)await _bookings.CountDocumentsAsync(b => b.Status == "Confirmed");
        }
    }
}