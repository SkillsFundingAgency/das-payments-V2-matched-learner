namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration
{
    public class ApplicationSettings
    {
        public string MatchedLearnerQueue { get; set; }
        public string PaymentsConnectionString { get; set; }
        public string MatchedLearnerConnectionString { get; set; }
        public string PaymentsServiceBusConnectionString { get; set; }
        public string AzureWebJobsStorage { get; set; }
        public string TargetUrl { get; set; }
        public string TimeToWait { get; set; }
        public string TimeToWaitUnexpected { get; set; }
        public string TimeToPause { get; set; }
        public bool IsDevelopment { get; set; }
    }
}
