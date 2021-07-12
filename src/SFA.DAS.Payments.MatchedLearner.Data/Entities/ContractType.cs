using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public enum ContractType: byte
    {
        None = byte.MaxValue, 
        Act1 = 1,
        Act2 = 2,
    }
}