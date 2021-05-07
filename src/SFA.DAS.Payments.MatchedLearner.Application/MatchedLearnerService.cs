using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Application.Repositories;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface IMatchedLearnerService
    {
        Task<MatchedLearnerDto> GetMatchedLearner(long ukprn, long uln);
    }

    public class MatchedLearnerService : IMatchedLearnerService
    {
        private readonly IPaymentsDataLockRepository _paymentsDataLockRepository;
        private readonly IMatchedLearnerDtoMapper _matchedLearnerDtoMapper;
        private readonly ILogger<MatchedLearnerService> _logger;

        public MatchedLearnerService(IPaymentsDataLockRepository paymentsDataLockRepository, IMatchedLearnerDtoMapper matchedLearnerDtoMapper, ILogger<MatchedLearnerService> logger)
        {
            _paymentsDataLockRepository = paymentsDataLockRepository ?? throw new ArgumentNullException(nameof(paymentsDataLockRepository));
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper ?? throw new ArgumentNullException(nameof(matchedLearnerDtoMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<MatchedLearnerDto> GetMatchedLearner(long ukprn, long uln)
        {
            try
            {
                var dataLockEvents = await _paymentsDataLockRepository.GetDataLockEvents(ukprn, uln);

                var matchedLearnerResult = _matchedLearnerDtoMapper.Map(dataLockEvents);

                return matchedLearnerResult;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error Getting MatchedLearner data for Uln {uln}", exception);
                throw;
            }
        }
    }
}
