using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.SqlAzureIdentityAuthentication
{
    public interface ISqlAzureIdentityTokenProvider
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
        string GetAccessToken();
    }
}