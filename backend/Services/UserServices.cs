namespace backend.Services
{
    using MongoDB.Driver;
    using backend.Models;
    using Microsoft.Extensions.Options;
    using backend.Interfaces;

    public class UserServices : IUserServices
    {
        private readonly IMongoCollection<User> _users;

        public UserServices(IMongoClient mongoClient, IOptions<MongoDbSettings> config)
        {
            var database = mongoClient.GetDatabase(config.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        public async Task CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;

            user.LastLogin = DateTime.MinValue;

            await _users.InsertOneAsync(user);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _users.ReplaceOneAsync(filter, user);
        }

    }


}
