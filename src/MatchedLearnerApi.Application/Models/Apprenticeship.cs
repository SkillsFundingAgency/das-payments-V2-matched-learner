namespace MatchedLearnerApi.Application.Models
{
    public class Apprenticeship
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long TransferSendingEmployerAccountId { get; set; }
        public byte ApprenticeshipEmployerType { get; set; }
    }
}
