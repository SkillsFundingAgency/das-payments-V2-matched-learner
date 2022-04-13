using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure
{
    public class TestAzureAdClientSettings : IAzureActiveDirectoryClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
    }
}