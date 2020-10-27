using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.ModelConfiguration
{
    public class DatalockEventPriceEpisodeConfiguration : IEntityTypeConfiguration<DatalockEventPriceEpisode>
    {
        public void Configure(EntityTypeBuilder<DatalockEventPriceEpisode> builder)
        {
            builder.ToTable("DataLockEventPriceEpisode");
        }
    }
}
