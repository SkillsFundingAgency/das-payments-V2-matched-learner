using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Types
{
    public class TrainingDto
    {
        // "Training" info
        public string Reference { get; set; }
        public int ProgrammeType { get; set; }
        public int StandardCode { get; set; }
        public int FrameworkCode { get; set; }
        public int PathwayCode { get; set; }
        public string FundingLineType { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; }


        public List<PriceEpisodeDto> PriceEpisodes { get; set; } = new List<PriceEpisodeDto>();
    }
}
