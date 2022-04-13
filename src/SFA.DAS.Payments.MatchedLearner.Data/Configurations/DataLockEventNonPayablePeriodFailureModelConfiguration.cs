using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class DataLockEventNonPayablePeriodFailureModelConfiguration : IEntityTypeConfiguration<DataLockEventNonPayablePeriodFailureModel>
    {
        public void Configure(EntityTypeBuilder<DataLockEventNonPayablePeriodFailureModel> builder)
        {
            builder.ToTable("DataLockEventNonPayablePeriodFailures", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id").IsRequired();
            builder.Property(x => x.DataLockFailureId).HasColumnName("DataLockFailureId").HasColumnType("TinyInt").IsRequired();
            builder.Property(x => x.DataLockEventNonPayablePeriodId).HasColumnName("DataLockEventNonPayablePeriodId").IsRequired();
            builder.Property(x => x.ApprenticeshipId).HasColumnName("ApprenticeshipId");
        }
    }
}