using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Data.Configurations;
using SFA.DAS.Payments.MatchedLearner.Data.Entities;

namespace SFA.DAS.Payments.MatchedLearner.Data.Contexts
{
    public class PaymentsDataContext : DbContext
    {
        public PaymentsDataContext (DbContextOptions options) : base(options)
        { }

        public DbSet<ApprenticeshipModel> Apprenticeship { get; set; }
        public DbSet<DataLockEventModel> DataLockEvent { get; set; }
        public DbSet<DataLockEventPriceEpisodeModel> DataLockEventPriceEpisode { get; set; }
        public DbSet<DataLockEventPayablePeriodModel> DataLockEventPayablePeriod { get; set; }
        public DbSet<DataLockEventNonPayablePeriodModel> DataLockEventNonPayablePeriod { get; set; }
        public DbSet<DataLockEventNonPayablePeriodFailureModel> DataLockEventNonPayablePeriodFailure { get; set; }

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