using Attendify.Common.Domain.FacialRecognition;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class FacialProfileConfiguration : AuditableConfiguration<FacialProfile>
{
    public override void PostConfigure(EntityTypeBuilder<FacialProfile> builder)
    {
        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();
    }
}
