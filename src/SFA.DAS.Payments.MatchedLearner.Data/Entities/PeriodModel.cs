namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class PeriodModel
    {
        public long Id { get; set; }
        public long PriceEpisodeId { get; set; }
        public bool IsPayable { get; set; }
        public short TransactionType { get; set; }
        public int Period { get; set; }
        public decimal Amount { get; set; }
        public long? ApprenticeshipId { get; set; }
        public long? AccountId { get; set; }
        public int? ApprenticeshipEmployerType { get; set; }
        public long? TransferSenderAccountId { get; set; }
        public bool FailedDataLock1 { get; set; }
        public bool FailedDataLock2 { get; set; }
        public bool FailedDataLock3 { get; set; }
        public bool FailedDataLock4 { get; set; }
        public bool FailedDataLock5 { get; set; }
        public bool FailedDataLock6 { get; set; }
        public bool FailedDataLock7 { get; set; }
        public bool FailedDataLock8 { get; set; }
        public bool FailedDataLock9 { get; set; }
        public bool FailedDataLock10 { get; set; }
        public bool FailedDataLock11 { get; set; }
        public bool FailedDataLock12 { get; set; }
    }
}