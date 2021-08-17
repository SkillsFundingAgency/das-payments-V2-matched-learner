using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure
{
    public class TestApplicationSettings : ApplicationSettings
    {
        public string MatchedLearnerAcceptanceTestConnectionString { get; set; }
        public string AzureWebJobsStorage { get; set; }
        public string TargetUrl { get; set; }
        public string TimeToWait { get; set; }
        public string TimeToWaitUnexpected { get; set; }
        public string TimeToPause { get; set; }
    }
}