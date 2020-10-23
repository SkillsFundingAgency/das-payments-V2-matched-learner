using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchedLearnerApi.Application.ModelConfiguration
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