using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SFA.DAS.Payments.MatchedLearner.Data.Configurations;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Contexts
{
    public interface IMatchedLearnerDataContext
    {
        DbSet<ApprenticeshipModel> Apprenticeship { get; set; }
        DatabaseFacade Database { get; }
        DbSet<DataLockEventModel> DataLockEvent { get; set; }
        DbSet<DataLockEventNonPayablePeriodModel> DataLockEventNonPayablePeriod { get; set; }
        DbSet<DataLockEventNonPayablePeriodFailureModel> DataLockEventNonPayablePeriodFailures { get; set; }
        DbSet<DataLockEventPayablePeriodModel> DataLockEventPayablePeriod { get; set; }
        DbSet<DataLockEventPriceEpisodeModel> DataLockEventPriceEpisode { get; set; }
        Task<int> SaveChanges(CancellationToken cancellationToken = default(CancellationToken));
        Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod);
        Task RemoveApprenticeships(IEnumerable<long> apprenticeshipIds);
    }

    public class MatchedLearnerDataContext : DbContext, IMatchedLearnerDataContext
    {
        public MatchedLearnerDataContext(DbContextOptions options) : base(options)
        { }

        public DbSet<ApprenticeshipModel> Apprenticeship { get; set; }
        public DbSet<DataLockEventModel> DataLockEvent { get; set; }
        public DbSet<DataLockEventNonPayablePeriodModel> DataLockEventNonPayablePeriod { get; set; }
        public DbSet<DataLockEventNonPayablePeriodFailureModel> DataLockEventNonPayablePeriodFailures { get; set; }
        public DbSet<DataLockEventPayablePeriodModel> DataLockEventPayablePeriod { get; set; }
        public DbSet<DataLockEventPriceEpisodeModel> DataLockEventPriceEpisode { get; set; }

        public async Task RemovePreviousSubmissionsData(long ukprn, short academicYear, IList<byte> collectionPeriod)
        {
            await Database.ExecuteSqlInterpolatedAsync($"DELETE FROM Payments2.DataLockEvent WHERE ukprn = {ukprn} AND AcademicYear = {academicYear} AND CollectionPeriod IN { string.Join(",", collectionPeriod)}");
        }

        public async Task RemoveApprenticeships(IEnumerable<long> apprenticeshipIds)
        {
            await Database.ExecuteSqlInterpolatedAsync($"DELETE FROM Payments2.Apprenticeship WHERE id IN { string.Join(",", apprenticeshipIds )}");
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
        }

        public async Task<int> SaveChanges(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}