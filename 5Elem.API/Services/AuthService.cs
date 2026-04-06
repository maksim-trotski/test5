using Dapper;
using Microsoft.IdentityModel.Tokens;
using _5Elem.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using _5Elem.API.Services.Interfaces;

namespace _5Elem.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDatabaseService _database;
        private readonly IConfiguration _configuration;

        public AuthService(IDatabaseService database, IConfiguration configuration)
        {
            _database = database;
            _configuration = configuration;
        }

        public async Task<AuthResponseModel> LoginAsync(string username, string password)
        {
            using var connection = _database.CreateConnection();

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT Id, Username, Email, PasswordHash FROM Users WHERE Username = @Username",
                new { Username = username });

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user);

            return new AuthResponseModel
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<AuthResponseModel> RegisterAsync(string username, string email, string password)
        {
            using var connection = _database.CreateConnection();

            var existing = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT Id FROM Users WHERE Email = @Email",
                new { Email = email });

            if (existing != null)
                return null;

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password)
            };

            var id = await connection.ExecuteScalarAsync<int>(
                "INSERT INTO Users (Username, Email, PasswordHash) VALUES (@Username, @Email, @PasswordHash); SELECT CAST(SCOPE_IDENTITY() as int)",
                user);

            user.Id = id;

            var token = GenerateJwtToken(user);

            return new AuthResponseModel
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
