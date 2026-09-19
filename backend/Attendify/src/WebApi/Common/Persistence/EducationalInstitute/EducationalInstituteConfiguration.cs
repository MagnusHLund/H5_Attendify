using Attendify.Common.Domain.EducationalInstitute;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class EducationalInstituteConfiguration : AuditableConfiguration<EducationalInstitute>
{
    public override void PostConfigure(EntityTypeBuilder<EducationalInstitute> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(id => id.Value, value => EducationalInstituteId.From(value));

        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
    }
}
