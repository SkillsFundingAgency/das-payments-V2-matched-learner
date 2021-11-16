namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration
{
    public class ApplicationSettings
    {
        public string MatchedLearnerQueue { get; set; }
        public string PaymentsConnectionString { get; set; }
        public string MatchedLearnerConnectionString { get; set; }
        public string PaymentsServiceBusConnectionString { get; set; }
        public bool IsDevelopment { get; set; }
        public string MigrationQueue { get; set; }
    }
}
