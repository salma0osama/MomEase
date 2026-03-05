using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MomEase.infra.Data
{
    public class MomEaseDbContextFactory : IDesignTimeDbContextFactory<MomEaseDbContext>
    {
        public MomEaseDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MomEaseDbContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=MomEaseDB;Trusted_Connection=True;TrustServerCertificate=True;");
            return new MomEaseDbContext(optionsBuilder.Options);
        }
    }
}