using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Data.Configurations;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Contexts
{
    public interface IMatchedLearnerContext
    {
        DbSet<DataLockEventModel> DataLockEvent { get; set; }
        DbSet<DataLockEventNonPayablePeriodModel> DataLockEventNonPayablePeriod { get; set; }
        DbSet<DataLockEventNonPayablePeriodFailureModel> DataLockEventNonPayablePeriodFailures { get; set; }
        DbSet<DataLockEventPayablePeriodModel> DataLockEventPayablePeriod { get; set; }
        DbSet<DataLockEventPriceEpisodeModel> DataLockEventPriceEpisode { get; set; }
        DbSet<ApprenticeshipModel> Apprenticeship { get; set; }
    }

    public class MatchedLearnerContext : DbContext, IMatchedLearnerContext
    {
        public MatchedLearnerContext(DbContextOptions options) : base(options)
        { }

        public DbSet<DataLockEventModel> DataLockEvent { get; set; }
        public DbSet<DataLockEventNonPayablePeriodModel> DataLockEventNonPayablePeriod { get; set; }
        public DbSet<DataLockEventNonPayablePeriodFailureModel> DataLockEventNonPayablePeriodFailures { get; set; }
        public DbSet<DataLockEventPayablePeriodModel> DataLockEventPayablePeriod { get; set; }
        public DbSet<DataLockEventPriceEpisodeModel> DataLockEventPriceEpisode { get; set; }
        public DbSet<ApprenticeshipModel> Apprenticeship { get; set; }

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
    }
}