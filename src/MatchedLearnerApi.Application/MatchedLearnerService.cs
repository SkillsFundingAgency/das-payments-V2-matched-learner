using System.Threading.Tasks;
using MatchedLearnerApi.Application.Mappers;
using MatchedLearnerApi.Application.Repositories;
using MatchedLearnerApi.Types;

namespace MatchedLearnerApi.Application
{
    public interface IMatchedLearnerService
    {
        Task<MatchedLearnerDto> GetMatchedLearner(long ukprn, long uln);
    }

    public class MatchedLearnerService : IMatchedLearnerService
    {
        private readonly IPaymentsDataLockRepository _paymentsDataLockRepository;
        private readonly IMatchedLearnerDtoMapper _matchedLearnerDtoMapper;

        public MatchedLearnerService(IPaymentsDataLockRepository paymentsDataLockRepository, IMatchedLearnerDtoMapper matchedLearnerDtoMapper)
        {
            _paymentsDataLockRepository = paymentsDataLockRepository;
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper;
        }
        public async Task<MatchedLearnerDto> GetMatchedLearner(long ukprn, long uln)
        {
            var dataLockEvents = await _paymentsDataLockRepository.GetDatalockEvents(ukprn, uln);

            var matchedLearnerResult = _matchedLearnerDtoMapper.Map(dataLockEvents);

            return matchedLearnerResult;
        }
    }
}
