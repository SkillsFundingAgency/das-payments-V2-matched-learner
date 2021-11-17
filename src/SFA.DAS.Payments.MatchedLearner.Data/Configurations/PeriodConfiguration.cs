using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class PeriodConfiguration : IEntityTypeConfiguration<PeriodModel>
    {
        public void Configure(EntityTypeBuilder<PeriodModel> builder)
        {
            builder.ToTable("Period", "dbo");
            builder.HasKey(x => x.Id);
        }
    }
}