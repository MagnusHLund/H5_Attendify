using Attendify.Common.Domain.EducationalInstitute;

namespace Attendify.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<EducationalInstitute> EducationalInstitutes =>
        AggregateRootSet<EducationalInstitute>();
}
