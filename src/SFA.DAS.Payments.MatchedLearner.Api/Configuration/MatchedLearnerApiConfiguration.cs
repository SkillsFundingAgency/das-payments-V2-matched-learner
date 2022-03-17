namespace SFA.DAS.Payments.MatchedLearner.Api.Configuration
{
    public interface IMatchedLearnerApiConfiguration
    {
        string DasPaymentsDatabaseConnectionString { get; set; }
        string TargetUrl { get; set; }
    }

    public class MatchedLearnerApiConfiguration : IMatchedLearnerApiConfiguration
    {
        public string DasPaymentsDatabaseConnectionString { get; set; }
        public string TargetUrl { get; set; }
    }
}
