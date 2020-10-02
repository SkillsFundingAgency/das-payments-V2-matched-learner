﻿using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatchedLearnerApi.Application.ModelConfiguration
{
    class DatalockEventNonPayablePeriodFailureConfiguration : IEntityTypeConfiguration<DatalockEventNonPayablePeriodFailure>
    {
        public void Configure(EntityTypeBuilder<DatalockEventNonPayablePeriodFailure> builder)
        {
            builder.HasOne(x => x.Apprenticeship)
                .WithMany()
                .HasForeignKey(x => x.ApprenticeshipId);
        }
    }
}
