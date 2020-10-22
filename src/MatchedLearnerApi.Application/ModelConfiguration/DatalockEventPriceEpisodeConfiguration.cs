using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchedLearnerApi.Application.ModelConfiguration
{
    public class DatalockEventPriceEpisodeConfiguration : IEntityTypeConfiguration<DatalockEventPriceEpisode>
    {
        public void Configure(EntityTypeBuilder<DatalockEventPriceEpisode> builder)
        {
            builder.ToTable("DataLockEventPriceEpisode");

            builder.Property(x => x.Identifier)
                .HasColumnName("PriceEpisodeIdentifier");

            builder.HasMany(x => x.NonPayablePeriods)
                .WithOne()
                .HasForeignKey(x => new {x.DataLockEventId, x.PriceEpisodeIdentifier})
                .HasPrincipalKey(x => new {x.DataLockEventId, x.Identifier});

            builder.HasMany(x => x.PayablePeriods)
                .WithOne()
                .HasForeignKey(x => new {x.DataLockEventId, x.PriceEpisodeIdentifier})
                .HasPrincipalKey(x => new {x.DataLockEventId, x.Identifier});
        }
    }
}
