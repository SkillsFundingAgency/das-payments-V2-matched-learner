using MatchedLearnerApi.Interfaces;

namespace MatchedLearnerApi.Configuration
{
    public class MatchedLearnerApiConfiguration : IMatchedLearnerApiConfiguration
    {
        public string DasPaymentsDatabaseConnectionString { get; set; }
        public string TargetUrl { get; set; }
    }
}
