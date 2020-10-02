namespace MatchedLearnerApi.Application.Models
{
    public class Apprenticeship
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long TransferSenderAccountId { get; set; }
        public int ApprenticeshipEmployerType { get; set; }
    }
}
