using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.MatchedLearner.Data.Entities
{
    public class MigrationStatusModel
    {
        public long Id { get; set; }
        public Guid? Identifier { get; set; }
        public long? Ukprn { get; set; }
        public MigrationStatus? Status { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}
