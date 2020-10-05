using MatchedLearnerApi.Application.ModelConfiguration;
using MatchedLearnerApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MatchedLearnerApi.Application
{
    public class PaymentsContext : DbContext, IPaymentsContext
    {
        private readonly string _connectionString;

        public PaymentsContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PaymentsConnectionString");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public DbSet<DatalockEvent> DatalockEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Payments2");

            modelBuilder.ApplyConfiguration(new DatalockEventConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventNonPayablePeriodConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventNonPayablePeriodFailureConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventPayablePeriodConfiguration());
            modelBuilder.ApplyConfiguration(new DatalockEventPriceEpisodeConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
