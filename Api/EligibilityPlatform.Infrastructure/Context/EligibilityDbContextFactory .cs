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

            var connectionString = "Server=110.226.124.45;Port=3306;Database=3MEligibility;User=3MEligibilityUser;Password=3MEligibilityUser#10012026;";

            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new EligibilityDbContext(optionsBuilder.Options);
        }
    }
}