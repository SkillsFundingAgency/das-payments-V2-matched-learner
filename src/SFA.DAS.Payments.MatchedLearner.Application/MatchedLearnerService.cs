using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.MatchedLearner.Application.Mappers;
using SFA.DAS.Payments.MatchedLearner.Data.Repositories;
using SFA.DAS.Payments.MatchedLearner.Types;

namespace SFA.DAS.Payments.MatchedLearner.Application
{
    public interface IMatchedLearnerService
    {
        Task<MatchedLearnerDto> GetMatchedLearner(long ukprn, long uln);
    }

    public class MatchedLearnerService : IMatchedLearnerService
    {
        private readonly IMatchedLearnerRepository _matchedLearnerRepository;
        private readonly IMatchedLearnerDtoMapper _matchedLearnerDtoMapper;
        private readonly ILogger<MatchedLearnerService> _logger;

        public MatchedLearnerService(IMatchedLearnerRepository matchedLearnerRepository, IMatchedLearnerDtoMapper matchedLearnerDtoMapper, ILogger<MatchedLearnerService> logger)
        {
            _matchedLearnerRepository = matchedLearnerRepository ?? throw new ArgumentNullException(nameof(matchedLearnerRepository));
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper ?? throw new ArgumentNullException(nameof(matchedLearnerDtoMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MatchedLearnerDto> GetMatchedLearner(long ukprn, long uln)
        {
            try
            {
                _logger.LogInformation($"Start GetMatchedLearner for Uln {uln}");

                var dataLockEvents = await _matchedLearnerRepository.GetDataLockEvents(ukprn, uln);

                var matchedLearnerResult = _matchedLearnerDtoMapper.Map(dataLockEvents);

                _logger.LogInformation($"End GetMatchedLearner for Uln {uln}");

                return matchedLearnerResult;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Error GetMatchedLearner for Uln {uln}");
                throw;
            }
        }
    }
}