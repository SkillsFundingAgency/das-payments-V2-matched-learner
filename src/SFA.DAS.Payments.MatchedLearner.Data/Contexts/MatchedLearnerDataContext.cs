using System.Collections.Generic;
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
        public DbSet<MigrationRunAttemptModel> MigrationRunAttempts { get; set; }
        public DbSet<TrainingModel> Trainings { get; set; }
        public DbSet<PriceEpisodeModel> PriceEpisodes { get; set; }
        public DbSet<PeriodModel> Periods { get; set; }

        public async Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod)
        {

            var sqlParameters = collectionPeriod.Select((item, index) => new SqlParameter($"@period{index}", item)).ToList();
            var sqlParamName = string.Join(", ", sqlParameters.Select(pn => pn.ParameterName));

            sqlParameters.Add(new SqlParameter("@ukprn", ukprn));
            sqlParameters.Add(new SqlParameter("@academicYear", academicYear));

            await Database.ExecuteSqlRawAsync($"DELETE FROM dbo.Training WHERE ukprn = @ukprn AND AcademicYear = @academicYear AND IlrSubmissionWindowPeriod IN ({ sqlParamName })", sqlParameters);
        }

        public async Task RemoveApprenticeships(IEnumerable<long> apprenticeshipIds)
        {
            var sqlParameters = apprenticeshipIds.Select((item, index) => new SqlParameter($"@Id{index}", item)).ToList();
            var sqlParamName = string.Join(", ", sqlParameters.Select(pn => pn.ParameterName));

            await Database.ExecuteSqlRawAsync($"DELETE FROM Payments2.Apprenticeship WHERE id IN ( {sqlParamName} )", sqlParameters);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ApprenticeshipModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventNonPayablePeriodFailureModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventNonPayablePeriodModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventPayablePeriodModelConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockEventPriceEpisodeModelConfiguration());
            modelBuilder.ApplyConfiguration(new MigrationRunAttemptConfiguration());
            modelBuilder.ApplyConfiguration(new TrainingConfiguration());
            modelBuilder.ApplyConfiguration(new PriceEpisodeConfiguration());
            modelBuilder.ApplyConfiguration(new PeriodConfiguration());
        }
    }
}