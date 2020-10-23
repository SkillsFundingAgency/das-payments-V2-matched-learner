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
        }
    }
}
