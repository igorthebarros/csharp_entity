using EntityFC.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EntityFC.Data
{
    public class ContextSecondary : DbContext
    {
        public DbSet<City> Cities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            const string connectionString = "Data source=(localdb)\\mssqllocaldb; Initial Catalog=EntityFC; Integrated Security=true;";

            builder
                .UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);

            //base.OnConfiguring(builder);
        }
    }
}
