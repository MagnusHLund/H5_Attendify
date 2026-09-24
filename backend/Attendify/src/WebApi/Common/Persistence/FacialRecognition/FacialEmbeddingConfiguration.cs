using Attendify.Common.Domain.FacialRecognition;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class FacialEmbeddingConfiguration : AuditableConfiguration<FacialEmbedding>
{
    public override void PostConfigure(EntityTypeBuilder<FacialEmbedding> builder)
    {
        builder.HasKey(embedding => embedding.Id);

        builder.Property(embedding => embedding.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder.Property(embedding => embedding.EncryptedEmbedding).IsRequired();
        builder.Property(embedding => embedding.Nonce).IsRequired();

        builder.HasOne(embedding => embedding.FacialProfile)
            .WithMany(profile => profile.FacialEmbeddings)
            .HasForeignKey(embedding => embedding.FacialProfileId)
            .IsRequired();
    }
}
