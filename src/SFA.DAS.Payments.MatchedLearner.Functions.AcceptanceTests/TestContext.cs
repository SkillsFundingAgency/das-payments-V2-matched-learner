using System;

namespace SFA.DAS.Payments.MatchedLearner.Functions.AcceptanceTests
{
    public class TestContext
    {
        public TestContext()
        {
            var random = new Random();

            Ukprn = random.Next(100000);
            LearnerUln = random.Next(100000);
            ApprenticeshipId = Ukprn + LearnerUln;
        }


        public TestRepository TestRepository { get; set; }
        public TestFunctionHost TestFunctionHost { get; set; }
        public TestEndpoint TestEndpointInstance { get; set; }
        public Guid? ExistingMatchedLearnerDataLockId { get; set; }
        public long ExistingMatchedLearnerTrainingId { get; set; }

        public long Ukprn { get; }
        public long LearnerUln { get; }
        public long ApprenticeshipId { get; }
    }
}