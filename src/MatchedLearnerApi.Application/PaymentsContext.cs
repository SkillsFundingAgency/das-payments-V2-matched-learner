using MatchedLearnerApi.Application.ModelConfiguration;
using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace MatchedLearnerApi.Application
{
    public class PaymentsContext : DbContext, IPaymentsContext
    {
        public PaymentsContext(DbContextOptions options) : base(options)
        { }

        public DbSet<DatalockEvent> DatalockEvents { get; set; }
        public virtual DbSet<LatestSuccessfulJobModel> LatestSuccessfulJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Payments2");

            modelBuilder.ApplyConfiguration(new LatestSuccessfulJobModelConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventNonPayablePeriodConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventNonPayablePeriodFailureConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventPayablePeriodConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventPriceEpisodeConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
