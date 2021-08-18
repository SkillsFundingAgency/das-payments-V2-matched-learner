using System;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure
{
    
    public class TestApplicationSettings : ApplicationSettings
    {
        public string MatchedLearnerAcceptanceTestConnectionString { get; set; }
        public string AzureWebJobsStorage { get; set; }
        public TimeSpan TimeToWait { get; set; }
        public TimeSpan TimeToWaitUnexpected { get; set; }
        public TimeSpan TimeToPause { get; set; }
    }

    public class TestAzureAdClientSettings : IAzureActiveDirectoryClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
    }
}