﻿using System;
using System.Collections.Generic;

namespace MatchedLearnerApi.Application.Models
{
    public class DatalockEventNonPayablePeriod
    {
        public long Id { get; set; }
        public Guid DataLockEventId { get; set; }
        public Guid DataLockEventNonPayablePeriodId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        
        public byte Period { get; set; }
        public virtual List<DatalockEventNonPayablePeriodFailure> Failures { get; set; } = new List<DatalockEventNonPayablePeriodFailure>();
    }
}
