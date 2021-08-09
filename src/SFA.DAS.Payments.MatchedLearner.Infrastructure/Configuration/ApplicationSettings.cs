using System;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration
{
    public interface IApplicationSettings
    {
        string ServiceName { get; set; }
        string MatchedLearnerQueue { get; set; }
        string PaymentsConnectionString { get; set; }
        string MatchedLearnerConnectionString { get; set; }
        string MatchedLearnerServiceBusConnectionString { get; set; }
        string AzureWebJobsStorage { get; set; }
        string TargetUrl { get; set; }
        string TimeToWait { get; set; }
        string TimeToWaitUnexpected { get; set; }
        string TimeToPause { get; set; }
    }

    public class ApplicationSettings : IApplicationSettings
    {
        public string ServiceName { get; set; }
        public string MatchedLearnerQueue { get; set; }
        public string PaymentsConnectionString { get; set; }
        public string MatchedLearnerConnectionString { get; set; }
        public string MatchedLearnerServiceBusConnectionString { get; set; }
        public string AzureWebJobsStorage { get; set; }
        public string TargetUrl { get; set; }
        public string TimeToWait { get; set; }
        public string TimeToWaitUnexpected { get; set; }
        public string TimeToPause { get; set; }
    }
}
