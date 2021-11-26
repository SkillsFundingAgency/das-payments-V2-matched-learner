namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public enum MigrationStatus : byte
    {
        InProgress = 1,
        Completed = 2,
        Failed = 3,
        CompletedWithErrors = 4
    }
}
