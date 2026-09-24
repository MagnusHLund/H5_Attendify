using Attendify.Common.Domain.FacialRecognition;

namespace Attendify.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<FacialProfile> FacialProfiles => AggregateRootSet<FacialProfile>();

    public DbSet<FacialEmbedding> FacialEmbeddings => AggregateRootSet<FacialEmbedding>();
}
