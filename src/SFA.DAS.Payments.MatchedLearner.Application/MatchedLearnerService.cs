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
        private readonly ILegacyMatchedLearnerRepository _legacyMatchedLearnerRepository;
        private readonly ILegacyMatchedLearnerDtoMapper _legacyMatchedLearnerDtoMapper;
        private readonly ILogger<MatchedLearnerService> _logger;
        private readonly bool _useV1Api;

        public MatchedLearnerService(IMatchedLearnerRepository matchedLearnerRepository, IMatchedLearnerDtoMapper matchedLearnerDtoMapper, ILegacyMatchedLearnerRepository legacyMatchedLearnerRepository, ILegacyMatchedLearnerDtoMapper legacyMatchedLearnerDtoMapper, ILogger<MatchedLearnerService> logger, bool useV1Api)
        {
            _matchedLearnerRepository = matchedLearnerRepository ?? throw new ArgumentNullException(nameof(matchedLearnerRepository));
            _matchedLearnerDtoMapper = matchedLearnerDtoMapper ?? throw new ArgumentNullException(nameof(matchedLearnerDtoMapper));
            _legacyMatchedLearnerRepository = legacyMatchedLearnerRepository ?? throw new ArgumentNullException(nameof(legacyMatchedLearnerRepository));
            _legacyMatchedLearnerDtoMapper = legacyMatchedLearnerDtoMapper ?? throw new ArgumentNullException(nameof(legacyMatchedLearnerDtoMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _useV1Api = useV1Api;
        }

        public async Task<MatchedLearnerDto> GetMatchedLearner(long ukprn, long uln)
        {
            try
            {
                _logger.LogInformation($"Start GetMatchedLearner for Uln {uln}");

                if (_useV1Api)
                {
                    var matchedLearnerTrainingsV1 = await _legacyMatchedLearnerRepository.GetDataLockEvents(ukprn, uln);
                    return _legacyMatchedLearnerDtoMapper.Map(matchedLearnerTrainingsV1);
                }

                var matchedLearnerTrainings = await _matchedLearnerRepository.GetMatchedLearnerTrainings(ukprn, uln);
                var matchedLearnerResult = _matchedLearnerDtoMapper.MapToDto(matchedLearnerTrainings);

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