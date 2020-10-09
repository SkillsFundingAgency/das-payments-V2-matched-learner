using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchedLearnerApi.Interfaces;

namespace MatchedLearnerApi.Configuration
{
    public class MatchedLearnerApiConfiguration : IMatchedLearnerApiConfiguration
    {
        public string DasPaymentsDatabaseConnectionString { get; set; }
    }
}
