﻿using MatchedLearnerApi.Application.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchedLearnerApi.Application.Data.ModelConfiguration
{
    public class DatalockEventConfiguration : IEntityTypeConfiguration<DatalockEvent>
    {
        public void Configure(EntityTypeBuilder<DatalockEvent> builder)
        {
            builder.ToTable("DataLockEvent");

            builder.HasMany(x => x.PriceEpisodes)
                .WithOne()
                .HasForeignKey(x => x.DataLockEventId)
                .HasPrincipalKey(x => x.EventId);

            builder.HasMany(x => x.NonPayablePeriods)
                .WithOne()
                .HasForeignKey(x => x.DataLockEventId)
                .HasPrincipalKey(x => x.EventId);

            builder.HasMany(x => x.PayablePeriods)
                .WithOne()
                .HasForeignKey(x => x.DataLockEventId)
                .HasPrincipalKey(x => x.EventId);
        }
    }
}
