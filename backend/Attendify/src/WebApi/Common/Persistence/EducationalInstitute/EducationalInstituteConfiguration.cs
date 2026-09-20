using Attendify.Common.Domain.EducationalInstitute;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class EducationalInstituteConfiguration : AuditableConfiguration<EducationalInstitute>
{
    public override void PostConfigure(EntityTypeBuilder<EducationalInstitute> builder)
    {
        builder.HasKey(institute => institute.Id);

        builder.Property(institute => institute.Id)
            .ValueGeneratedOnAdd();

        builder.Property(institute => institute.Name)
            .HasMaxLength(EducationalInstitute.NameMaxLength)
            .IsRequired();

        builder.HasIndex(institute => institute.Name).IsUnique();
    }
}
