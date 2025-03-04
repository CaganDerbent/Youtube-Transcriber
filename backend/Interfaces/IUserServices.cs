using backend.Models;

namespace backend.Interfaces
{
    public interface IUserServices
    {
        Task CreateUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);
    }
}
