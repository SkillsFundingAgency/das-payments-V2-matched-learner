using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchedLearnerApi.Application.ModelConfiguration
{
    public class DatalockEventPayablePeriodConfiguration : IEntityTypeConfiguration<DatalockEventPayablePeriod>
    {
        public void Configure(EntityTypeBuilder<DatalockEventPayablePeriod> builder)
        {
            builder.Property(x => x.Period)
                .HasColumnName("DeliveryPeriod");

            builder.HasOne(x => x.Apprenticeship)
                .WithMany()
                .HasForeignKey(x => x.ApprenticeshipId);
        }
    }
}
