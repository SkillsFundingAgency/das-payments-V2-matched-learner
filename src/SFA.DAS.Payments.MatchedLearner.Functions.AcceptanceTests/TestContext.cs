using System;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public class TestContext
    {
        public TestRepository TestRepository { get; set; }
        public TestFunction TestFunction { get; set; }
        public TestEndpoint TestEndpointInstance { get; set; }
        public TimeSpan TimeToWait { get; set; }
        public TimeSpan TimeToPause { get; set; }
        public Guid? ExistingMatchedLearnerDataLockId { get; set; }
    }
}