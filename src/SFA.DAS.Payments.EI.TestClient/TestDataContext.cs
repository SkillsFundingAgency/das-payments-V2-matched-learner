using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.Payments.EI.TestClient
{
    public class TestDataContext : DbContext
    {
        public TestDataContext(DbContextOptions options) : base(options)
        { }

        public DbSet<ApprenticeshipInput> ApprenticeshipInput { get; set; }
        public DbSet<ApprenticeshipOutPut> ApprenticeshipOutPut { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Payments2");

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ApprenticeshipInputConfiguration());
            modelBuilder.ApplyConfiguration(new ApprenticeshipOutPutConfiguration());
        }
    }
}