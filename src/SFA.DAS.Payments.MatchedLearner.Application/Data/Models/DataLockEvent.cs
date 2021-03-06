﻿using System;

namespace SFA.DAS.Payments.MatchedLearner.Application.Data.Models
{
    public class DataLockEvent
    {
        public long Id { get; set; }
        public long JobId { get; set; }

        // "Header" info
        public Guid EventId { get; set; }
        public short AcademicYear { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public byte CollectionPeriod { get; set; }
        public long Ukprn { get; set; }
        public long LearnerUln { get; set; }


        // "Training" info
        public string LearningAimReference { get; set; }

        public int LearningAimProgrammeType { get; set; }
        public int LearningAimStandardCode { get; set; }
        public int LearningAimFrameworkCode { get; set; }
        public int LearningAimPathwayCode { get; set; }
        public string LearningAimFundingLineType { get; set; }
        
        public DateTime? LearningStartDate { get; set; }
    }
}
