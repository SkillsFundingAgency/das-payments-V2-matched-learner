using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.ModelConfiguration
{
    public class LatestSuccessfulJobModelConfiguration : IEntityTypeConfiguration<LatestSuccessfulJobModel>
    {
        public void Configure(EntityTypeBuilder<LatestSuccessfulJobModel> builder)
        {
            builder.ToTable("LatestSuccessfulJobs", "Payments2");
            
            builder.HasKey(e => e.JobId);
        }
    }
}