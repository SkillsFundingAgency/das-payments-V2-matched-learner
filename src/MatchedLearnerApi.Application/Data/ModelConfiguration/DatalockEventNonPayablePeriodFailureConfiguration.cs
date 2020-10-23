using MatchedLearnerApi.Application.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchedLearnerApi.Application.Data.ModelConfiguration
{
    public class DatalockEventNonPayablePeriodFailureConfiguration : IEntityTypeConfiguration<DatalockEventNonPayablePeriodFailure>
    {
        public void Configure(EntityTypeBuilder<DatalockEventNonPayablePeriodFailure> builder)
        {
            builder.ToTable("DatalockEventNonPayablePeriodFailures");

            builder.HasOne(x => x.Apprenticeship)
                .WithMany()
                .HasForeignKey(x => x.ApprenticeshipId);
        }
    }
}
