using System;
using SFA.DAS.Payments.MatchedLearner.Infrastructure.Configuration;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Infrastructure
{
    public class TestApplicationSettings : ApplicationSettings
    {
        public string MatchedLearnerStorageAccountConnectionString { get; set; }
        public TimeSpan TimeToWait { get; set; }
        public TimeSpan TimeToWaitUnexpected { get; set; }
        public TimeSpan TimeToPause { get; set; }
    }
}