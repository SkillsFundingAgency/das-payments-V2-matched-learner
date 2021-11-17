using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class TrainingConfiguration : IEntityTypeConfiguration<TrainingModel>
    {
        public void Configure(EntityTypeBuilder<TrainingModel> builder)
        {
            builder.ToTable("Training", "dbo");
            builder.HasKey(x => x.Id);
        }
    }
}