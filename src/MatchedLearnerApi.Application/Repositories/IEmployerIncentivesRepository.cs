using System.Threading.Tasks;

namespace MatchedLearnerApi.Application.Repositories
{
    public interface IEmployerIncentivesRepository
    {
        Task<MatchedLearnerResult> MatchedLearner(long ukprn, long uln);
    }
}