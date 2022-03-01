using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Application.Data.ModelConfiguration;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data
{
    public interface IPaymentsContext
    {
        DbSet<DataLockEvent> DataLockEvent { get; set; }
        DbSet<DataLockEventNonPayablePeriod> DataLockEventNonPayablePeriod { get; set; }
        DbSet<DataLockEventNonPayablePeriodFailure> DataLockEventNonPayablePeriodFailures { get; set; }
        DbSet<DataLockEventPayablePeriod> DataLockEventPayablePeriod { get; set; }
        DbSet<DataLockEventPriceEpisode> DataLockEventPriceEpisode { get; set; }
        DbSet<Apprenticeship> Apprenticeship { get; set; }
        DbSet<LatestSuccessfulJobModel> LatestSuccessfulJobs { get; set; }
    }

    public class PaymentsContext : DbContext, IPaymentsContext
    {
        public PaymentsContext(DbContextOptions options) : base(options)
        { }

        public DbSet<DataLockEvent> DataLockEvent { get; set; }
        public DbSet<DataLockEventNonPayablePeriod> DataLockEventNonPayablePeriod { get; set; }
        public DbSet<DataLockEventNonPayablePeriodFailure> DataLockEventNonPayablePeriodFailures { get; set; }
        public DbSet<DataLockEventPayablePeriod> DataLockEventPayablePeriod { get; set; }
        public DbSet<DataLockEventPriceEpisode> DataLockEventPriceEpisode { get; set; }
        public DbSet<Apprenticeship> Apprenticeship { get; set; }
        public DbSet<LatestSuccessfulJobModel> LatestSuccessfulJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Payments2");

            modelBuilder.ApplyConfiguration(new LatestSuccessfulJobModelConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
