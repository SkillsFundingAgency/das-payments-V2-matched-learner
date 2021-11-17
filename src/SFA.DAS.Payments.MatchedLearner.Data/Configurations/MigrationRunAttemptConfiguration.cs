﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class MigrationRunAttemptConfiguration : IEntityTypeConfiguration<MigrationRunAttemptModel>
    {
        public void Configure(EntityTypeBuilder<MigrationRunAttemptModel> builder)
        {
            builder.ToTable("MigrationStatus", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id").IsRequired();
            builder.Property(x => x.MigrationRunId).HasColumnName("MigrationRunId").IsRequired();
            builder.Property(x => x.Status).HasColumnName("Status").IsRequired();
            builder.Property(x => x.Ukprn).HasColumnName("Ukprn").IsRequired();
            builder.Property(x => x.CreationDate).HasColumnName("CreationDate").IsRequired();
        }
    }
}