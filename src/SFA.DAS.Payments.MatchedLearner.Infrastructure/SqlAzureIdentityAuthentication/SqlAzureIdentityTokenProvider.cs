using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.SqlAzureIdentityAuthentication
{
    public class SqlAzureIdentityTokenProvider : ISqlAzureIdentityTokenProvider
    {
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly ILogger<SqlAzureIdentityTokenProvider> _logger;

        public SqlAzureIdentityTokenProvider(AzureServiceTokenProvider azureServiceTokenProvider, ILogger<SqlAzureIdentityTokenProvider> logger)
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var token = await _azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/", cancellationToken: cancellationToken);

            _logger.LogInformation("Generated SQL AccessToken");

            return token;
        }

        public string GetAccessToken()
        {
            var token = _azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult();
            
            _logger.LogInformation("Generated SQL AccessToken");

            return token;
        }
    }
}