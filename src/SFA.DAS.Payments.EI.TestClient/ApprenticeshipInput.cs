using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SFA.DAS.Payments.EI.TestClient
{
    public class ApprenticeshipInput
    {
        public long Uln { get; set; }
        public long Ukprn { get; set; }
        public long Id { get; set; }
    }
    public class ApprenticeshipOutPut
    {
        public long Uln { get; set; }
        public long Ukprn { get; set; }
        public string LearnerJson { get; set; }
        public long Id { get; set; }
    }

    public class ApprenticeshipInputConfiguration : IEntityTypeConfiguration<ApprenticeshipInput>
    {
        public void Configure(EntityTypeBuilder<ApprenticeshipInput> builder)
        {
            builder.ToTable("ApprenticeshipInput", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id").IsRequired();
            builder.Property(x => x.Uln).HasColumnName("Uln").IsRequired();
            builder.Property(x => x.Ukprn).HasColumnName("Ukprn").IsRequired();
        }
    }
    
    public class ApprenticeshipOutPutConfiguration : IEntityTypeConfiguration<ApprenticeshipOutPut>
    {
        public void Configure(EntityTypeBuilder<ApprenticeshipOutPut> builder)
        {
            builder.ToTable("ApprenticeshipOutPut", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id").IsRequired();
            builder.Property(x => x.Uln).HasColumnName("Uln").IsRequired();
            builder.Property(x => x.Ukprn).HasColumnName("Ukprn").IsRequired();
            builder.Property(x => x.LearnerJson).HasColumnName("LearnerJson").IsRequired();
        }
    }
}