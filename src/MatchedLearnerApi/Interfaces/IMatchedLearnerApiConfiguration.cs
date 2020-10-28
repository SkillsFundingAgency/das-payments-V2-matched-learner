namespace MatchedLearnerApi.Interfaces
{
    public interface IMatchedLearnerApiConfiguration
    {
        string DasPaymentsDatabaseConnectionString { get; set; }
        string TargetUrl { get; set; }
    }
}
