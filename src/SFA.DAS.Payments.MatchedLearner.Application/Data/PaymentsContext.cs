using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.MatchedLearner.Application.Data.ModelConfiguration;
using SFA.DAS.Payments.MatchedLearner.Application.Data.Models;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data
{
    public interface IPaymentsContext
    {
        DbSet<DatalockEvent> DatalockEvents { get; set; }
        DbSet<LatestSuccessfulJobModel> LatestSuccessfulJobs { get; set; }
    }

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

            base.OnModelCreating(modelBuilder);
        }
    }
}
