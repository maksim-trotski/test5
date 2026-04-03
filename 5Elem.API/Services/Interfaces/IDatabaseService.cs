using System.Data;

namespace _5Elem.API.Services.Interfaces
{
    public interface IDatabaseService
    {
        IDbConnection CreateConnection();
    }
}
