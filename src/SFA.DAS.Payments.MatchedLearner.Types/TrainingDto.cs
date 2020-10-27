using System;
using System.Collections.Generic;

namespace SFA.DAS.Payments.MatchedLearner.Types
{
    public class TrainingDto
    {
        public string Reference { get; set; }
        public int ProgrammeType { get; set; }
        public int StandardCode { get; set; }
        public int FrameworkCode { get; set; }
        public int PathwayCode { get; set; }
        public string FundingLineType { get; set; }
        public DateTime StartDate { get; set; }

        public List<PriceEpisodeDto> PriceEpisodes { get; set; } = new List<PriceEpisodeDto>();
    }
}
