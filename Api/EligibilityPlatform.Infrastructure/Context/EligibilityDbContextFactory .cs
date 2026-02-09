using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MEligibilityPlatform.Infrastructure.Context
{
    public class EligibilityDbContextFactory : IDesignTimeDbContextFactory<EligibilityDbContext>
    {
        public EligibilityDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EligibilityDbContext>();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // project root
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new EligibilityDbContext(optionsBuilder.Options);
        }
    }
}
