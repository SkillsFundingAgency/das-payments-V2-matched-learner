﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Data.Configurations;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Contexts
{
    public class MatchedLearnerDataContext : DbContext
    {
        public MatchedLearnerDataContext(DbContextOptions options) : base(options)
        { }

        public DbSet<ApprenticeshipModel> Apprenticeship { get; set; }
        public DbSet<DataLockEventModel> DataLockEvent { get; set; }
        public DbSet<DataLockEventNonPayablePeriodModel> DataLockEventNonPayablePeriod { get; set; }
        public DbSet<DataLockEventNonPayablePeriodFailureModel> DataLockEventNonPayablePeriodFailures { get; set; }
        public DbSet<DataLockEventPayablePeriodModel> DataLockEventPayablePeriod { get; set; }
        public DbSet<DataLockEventPriceEpisodeModel> DataLockEventPriceEpisode { get; set; }
        public DbSet<SubmissionJobModel> SubmissionJobs { get; set; }

        public async Task RemovePreviousSubmissionsData(long ukprn, short academicYear, byte collectionPeriod)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@ukprn", ukprn),
                new SqlParameter("@academicYear", academicYear),
                new SqlParameter("@collectionPeriod", collectionPeriod),
            };

            const string sql = "DELETE FROM Payments2.DataLockEvent WHERE ukprn = @ukprn AND AcademicYear = @academicYear AND CollectionPeriod <= @collectionPeriod";
            
            await Database.ExecuteSqlRawAsync(sql, sqlParameters);
        }

        public async Task RemoveApprenticeships(IEnumerable<long> apprenticeshipIds)
        {
            var sqlParameters = apprenticeshipIds.Select((item, index) => new SqlParameter($"@Id{index}", item)).ToList();

            var sql = $"DELETE FROM Payments2.Apprenticeship WHERE id IN ( {string.Join(", ", sqlParameters.Select(pn => pn.ParameterName))} )";

            await Database.ExecuteSqlRawAsync(sql, sqlParameters); //NOSONAR
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Payments2");

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ApprenticeshipModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventNonPayablePeriodFailureModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventNonPayablePeriodModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventPayablePeriodModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventPriceEpisodeModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventPriceEpisodeModelConfiguration());
            modelBuilder.ApplyConfiguration(new SubmissionJobConfiguration());
        }
    }
}