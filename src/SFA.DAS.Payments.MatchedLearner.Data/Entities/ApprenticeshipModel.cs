using System;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class ApprenticeshipModel
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string AgreementId { get; set; }
        public DateTime AgreedOnDate { get; set; }
        public long Uln { get; set; }
        public long Ukprn { get; set; }
        public DateTime EstimatedStartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public long? StandardCode { get; set; }
        public int? ProgrammeType { get; set; }
        public int? FrameworkCode { get; set; }
        public int? PathwayCode { get; set; }
        public string LegalEntityName { get; set; }
        public long? TransferSendingEmployerAccountId { get; set; }
        public DateTime? StopDate { get; set; }
        public int Priority { get; set; }
        public ApprenticeshipStatus Status { get; set; }
        public bool IsLevyPayer { get; set; }

        // public List<ApprenticeshipPriceEpisodeModel> ApprenticeshipPriceEpisodes { get; set; }
        public byte ApprenticeshipEmployerType { get; set; }

        // public List<ApprenticeshipPauseModel> ApprenticeshipPauses { get; set; }

        public DateTimeOffset CreationDate { get; set; }
    }
}