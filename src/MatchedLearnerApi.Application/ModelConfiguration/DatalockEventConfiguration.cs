using System.Security.Cryptography.X509Certificates;
using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchedLearnerApi.Application.ModelConfiguration
{
    class DatalockEventConfiguration : IEntityTypeConfiguration<DatalockEvent>
    {
        public void Configure(EntityTypeBuilder<DatalockEvent> builder)
        {
            builder.ToTable("DataLockEvent");
            builder.Property(x => x.IlrSubmissionWindowPeriod)
                .HasColumnName("CollectionPeriod");

            builder.Property(x => x.Reference)
                .HasColumnName("LearningAimReference");
            builder.Property(x => x.ProgrammeType)
                .HasColumnName("LearningAimProgrammeType");
            builder.Property(x => x.StandardCode)
                .HasColumnName("LearningAimStandardCode");
            builder.Property(x => x.FrameworkCode)
                .HasColumnName("LearningAimFrameworkCode");
            builder.Property(x => x.PathwayCode)
                .HasColumnName("LearningAimPathwayCode");
            builder.Property(x => x.FundingLineType)
                .HasColumnName("LearningAimFundingLineType");
            builder.Property(x => x.Uln)
                .HasColumnName("LearnerUln");

            builder.HasMany(x => x.PriceEpisodes)
                .WithOne()
                .HasForeignKey(x => x.DataLockEventId)
                .HasPrincipalKey(x => x.EventId);
        }
    }
}
