using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatchedLearnerApi.Interfaces
{
    public interface IMatchedLearnerApiConfiguration
    {
        string DasPaymentsDatabaseConnectionString { get; set; }
        string LoggingConnectionString { get; set; }
        string LoggingKey { get; set; }
    }
}
