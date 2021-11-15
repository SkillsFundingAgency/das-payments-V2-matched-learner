using System;
using System.Collections.Generic;
using System.Text;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Configurations
{
    public class MigrationStatusConfiguration : IEntityTypeConfiguration<MigrationStatusModel>
    {
        public void Configure(EntityTypeBuilder<MigrationStatusModel> builder)
        {
            builder.ToTable("MigrationStatus", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id").IsRequired();
            builder.Property(x => x.Identifier).HasColumnName("Identifier").IsRequired();
            builder.Property(x => x.Status).HasColumnName("Status").IsRequired();
            builder.Property(x => x.Ukprn).HasColumnName("Ukprn").IsRequired();
            builder.Property(x => x.CreationDate).HasColumnName("CreationDate").IsRequired();
        }
    }
}
