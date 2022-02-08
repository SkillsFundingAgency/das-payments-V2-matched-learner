namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration
{
    public class ApplicationSettings
    {
        public bool IsDevelopment { get; set; }
        public bool ConnectionNeedsAccessToken { get; set; }
        
        public string PaymentsConnectionString { get; set; }
        public string MatchedLearnerConnectionString { get; set; }
        public string PaymentsServiceBusConnectionString { get; set; }
        public string MatchedLearnerServiceBusConnectionString { get; set; }

        public string NServiceBusLicense { get; set; }
        
        public string MatchedLearnerQueue { get; set; }
        public string MatchedLearnerImportQueue { get; set; }
        public string MatchedLearnerImportDelay { get; set; }
    }
}
