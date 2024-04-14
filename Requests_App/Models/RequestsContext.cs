using Azure.Core;
using Microsoft.EntityFrameworkCore;

namespace Requests_App.Models
{
    public class RequestsContext : DbContext
    {
        public DbSet<Request> Requests { get; set; }
        public RequestsContext(DbContextOptions options) : base(options)
        {

        }
    }
}
