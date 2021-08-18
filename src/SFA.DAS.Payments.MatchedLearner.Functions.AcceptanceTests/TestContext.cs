using System;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public class TestContext
    {
        public TestRepository TestRepository { get; set; }
        public TestFunctionHost TestFunctionHost { get; set; }
        public TestEndpoint TestEndpointInstance { get; set; }
        public Guid? ExistingMatchedLearnerDataLockId { get; set; }
    }
}