using Attendify.Common.Domain.EducationalInstitute;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class EducationalInstituteConfiguration : AuditableConfiguration<EducationalInstitute>
{
    private static readonly Guid ZbcRingstedId = Guid.Parse("8d6f2f7b-6c1b-4f6d-9f43-4e0e6c4c1a11");

    private static readonly Guid ZbcSlagelseId = Guid.Parse("f2b9a3c8-1e74-4c2a-8d91-7b5e6f3a2d22");

    private static readonly DateTimeOffset SeededAt = new(2026, 9, 21, 0, 0, 0, TimeSpan.Zero);

    public override void PostConfigure(EntityTypeBuilder<EducationalInstitute> builder)
    {
        builder.HasKey(institute => institute.Id);

        builder.Property(institute => institute.Id).ValueGeneratedOnAdd();

        builder
            .Property(institute => institute.Name)
            .HasMaxLength(EducationalInstitute.NameMaxLength)
            .IsRequired();

        builder.HasIndex(institute => institute.Name).IsUnique();

        builder.HasData(
            new
            {
                Id = ZbcRingstedId,
                Name = "ZBC - Ringsted",
                CreatedAt = SeededAt,
                CreatedBy = "System",
                UpdatedAt = SeededAt,
                UpdatedBy = "System",
            },
            new
            {
                Id = ZbcSlagelseId,
                Name = "ZBC - Slagelse",
                CreatedAt = SeededAt,
                CreatedBy = "System",
                UpdatedAt = SeededAt,
                UpdatedBy = "System",
            }
        );
    }
}
