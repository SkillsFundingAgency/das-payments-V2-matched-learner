using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class LatestSubmissionJobConfiguration : IEntityTypeConfiguration<LatestSubmissionJobModel>
    {
        public void Configure(EntityTypeBuilder<LatestSubmissionJobModel> builder)
        {
            builder.ToTable("LatestSubmissionJob", "dbo");
            builder.HasKey(x => x.Id);
        }
    }
}