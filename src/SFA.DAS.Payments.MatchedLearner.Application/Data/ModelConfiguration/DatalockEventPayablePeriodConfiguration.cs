using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.ModelConfiguration
{
    public class DatalockEventPayablePeriodConfiguration : IEntityTypeConfiguration<DatalockEventPayablePeriod>
    {
        public void Configure(EntityTypeBuilder<DatalockEventPayablePeriod> builder)
        {
            builder.ToTable("DatalockEventPayablePeriod");

            builder.HasOne(x => x.Apprenticeship)
                .WithMany()
                .HasForeignKey(x => x.ApprenticeshipId);
        }
    }
}
