using _5Elem.API.Models;

namespace _5Elem.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseModel> LoginAsync(string username, string password);
        Task<AuthResponseModel> RegisterAsync(string username, string email, string password);
    }
}
