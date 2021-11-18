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
        private readonly ILegacyMatchedLearnerRepository _legacyMatchedLearnerRepository;
        private readonly ILegacyMatchedLearnerDtoMapper _legacyMatchedLearnerDtoMapper;
        private readonly ILogger<MatchedLearnerService> _logger;

        public MatchedLearnerService(ILegacyMatchedLearnerRepository legacyMatchedLearnerRepository, ILegacyMatchedLearnerDtoMapper legacyMatchedLearnerDtoMapper, ILogger<MatchedLearnerService> logger)
        {
            _legacyMatchedLearnerRepository = legacyMatchedLearnerRepository ?? throw new ArgumentNullException(nameof(legacyMatchedLearnerRepository));
            _legacyMatchedLearnerDtoMapper = legacyMatchedLearnerDtoMapper ?? throw new ArgumentNullException(nameof(legacyMatchedLearnerDtoMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MatchedLearnerDto> GetMatchedLearner(long ukprn, long uln)
        {
            try
            {
                _logger.LogInformation($"Start GetMatchedLearner for Uln {uln}");

                var matchedLearnerTrainings = await _legacyMatchedLearnerRepository.GetDataLockEvents(ukprn, uln);
                var matchedLearnerResult = _legacyMatchedLearnerDtoMapper.Map(matchedLearnerTrainings);

                //TODO for Phase 3
                //var matchedLearnerTrainings = await _matchedLearnerRepository.GetMatchedLearnerTrainings(ukprn, uln);
                //var matchedLearnerResult = _MatchedLearnerDtoMapper.MapToDto(matchedLearnerTrainings);

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