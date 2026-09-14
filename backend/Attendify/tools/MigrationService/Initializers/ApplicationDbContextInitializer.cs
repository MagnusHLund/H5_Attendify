using Bogus;
using Microsoft.EntityFrameworkCore;
using Attendify.Common.Persistence;

namespace MigrationService.Initializers;

public class ApplicationDbContextInitializer(ApplicationDbContext dbContext) : DbContextInitializerBase<ApplicationDbContext>(dbContext)
{
    public void SeedDataType()
    {
    }
}