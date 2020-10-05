using System.Collections.Generic;
using MatchedLearnerApi.Application.Models;
using MatchedLearnerApi.Types;

namespace MatchedLearnerApi.Application.Repositories
{
    public interface IMatchedLearnerResultMapper
    {
        MatchedLearnerResultDto Map(List<DatalockEvent> datalockEvents);
    }
}