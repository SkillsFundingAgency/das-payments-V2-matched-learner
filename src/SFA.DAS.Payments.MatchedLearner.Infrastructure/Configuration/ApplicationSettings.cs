namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration
{
    public interface IApplicationSettings
    {
        string ServiceName { get; set; }
        string EndpointName { get; set; }
        
        string PaymentsConnectionString { get; set; }
        string MatchedLearnerConnectionString { get; set; }
        string ServiceBusConnectionString { get; set; }
        string DasNServiceBusLicenseKey { get; set; }
        string TargetUrl { get; set; }
    }

    public class ApplicationSettings : IApplicationSettings
    {
        public string ServiceName { get; set; }
        public string EndpointName { get; set; }
        
        public string PaymentsConnectionString { get; set; }
        public string MatchedLearnerConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string DasNServiceBusLicenseKey { get; set; }

        public string TargetUrl { get; set; }
    }
}
