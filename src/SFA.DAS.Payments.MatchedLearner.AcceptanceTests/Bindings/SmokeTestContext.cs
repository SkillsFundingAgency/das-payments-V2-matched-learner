using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.AcceptanceTests.Bindings
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SmokeTestContext
    {
        public Func<Task> FailedRequest { get; set; }
        public List<Func<Task>> Requests { get; set; } = new List<Func<Task>>();
        public MatchedLearnerDto MatchedLearnerDto { get; set; }
    }
}