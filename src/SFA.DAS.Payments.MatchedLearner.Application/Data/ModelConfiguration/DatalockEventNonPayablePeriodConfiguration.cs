using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.ModelConfiguration
{
    public class DatalockEventNonPayablePeriodConfiguration : IEntityTypeConfiguration<DatalockEventNonPayablePeriod>
    {
        public void Configure(EntityTypeBuilder<DatalockEventNonPayablePeriod> builder)
        {
            builder.ToTable("DatalockEventNonPayablePeriod");

            builder
                .HasMany(x => x.Failures)
                .WithOne()
                .HasForeignKey(x => x.DataLockEventNonPayablePeriodId)
                .HasPrincipalKey(x => x.DataLockEventNonPayablePeriodId);
        }
    }
}
