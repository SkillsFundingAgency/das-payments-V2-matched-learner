using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class SubmissionJobConfiguration : IEntityTypeConfiguration<SubmissionJobModel>
    {
        public void Configure(EntityTypeBuilder<SubmissionJobModel> builder)
        {
            builder.ToTable("SubmissionJob", "dbo");
            builder.HasKey(x => x.Id);
        }
    }
}