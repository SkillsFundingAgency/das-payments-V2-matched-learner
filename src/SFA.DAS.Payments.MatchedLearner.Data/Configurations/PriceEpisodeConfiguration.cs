using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class PriceEpisodeConfiguration : IEntityTypeConfiguration<PriceEpisodeModel>
    {
        public void Configure(EntityTypeBuilder<PriceEpisodeModel> builder)
        {
            builder.ToTable("PriceEpisode", "dbo");
            builder.HasKey(x => x.Id);
        }
    }
}