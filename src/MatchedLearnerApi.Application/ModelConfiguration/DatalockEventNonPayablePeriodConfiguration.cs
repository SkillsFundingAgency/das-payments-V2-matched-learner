using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchedLearnerApi.Application.ModelConfiguration
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
