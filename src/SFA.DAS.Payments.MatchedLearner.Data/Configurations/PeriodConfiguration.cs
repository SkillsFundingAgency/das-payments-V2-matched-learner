using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class PeriodConfiguration : IEntityTypeConfiguration<PeriodModel>
    {
        public void Configure(EntityTypeBuilder<PeriodModel> builder)
        {
            builder.ToTable("Period", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FailedDataLock1).HasColumnName("FailedDataLock1").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock2).HasColumnName("FailedDataLock2").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock3).HasColumnName("FailedDataLock3").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock4).HasColumnName("FailedDataLock4").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock5).HasColumnName("FailedDataLock5").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock6).HasColumnName("FailedDataLock6").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock7).HasColumnName("FailedDataLock7").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock8).HasColumnName("FailedDataLock8").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock9).HasColumnName("FailedDataLock9").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock10).HasColumnName("FailedDataLock10").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock11).HasColumnName("FailedDataLock11").HasDefaultValue(false);
            builder.Property(x => x.FailedDataLock12).HasColumnName("FailedDataLock12").HasDefaultValue(false);
        }
    }
}