using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class MigrationRunAttemptConfiguration : IEntityTypeConfiguration<MigrationRunAttemptModel>
    {
        public void Configure(EntityTypeBuilder<MigrationRunAttemptModel> builder)
        {
            builder.ToTable("MigrationRunAttempt", "Payments2");
            builder.HasKey(x => x.Id);
        }
    }
}
