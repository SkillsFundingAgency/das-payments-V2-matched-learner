using System.Threading.Tasks;
using MatchedLearnerApi.Types;

namespace MatchedLearnerApi.Application.Repositories
{
    public interface IEmployerIncentivesRepository
    {
        Task<MatchedLearnerResultDto> MatchedLearner(long ukprn, long uln);
    }
}