using _5Elem.API.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace _5Elem.API.Services
{
    public class AzureDatabaseService : IDatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AzureDatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("AzureSqlConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
