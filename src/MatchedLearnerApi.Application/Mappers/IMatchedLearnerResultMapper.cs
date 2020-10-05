using System.Collections.Generic;
using MatchedLearnerApi.Application.Models;
using MatchedLearnerApi.Types;

namespace MatchedLearnerApi.Application.Mappers
{
    public interface IMatchedLearnerResultMapper
    {
        MatchedLearnerResultDto Map(List<DatalockEvent> datalockEvents);
    }
}