using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.SqlAzureIdentityAuthentication
{
    public class SqlAzureIdentityAuthenticationDbConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly ILogger<SqlAzureIdentityAuthenticationDbConnectionInterceptor> _logger;
        private readonly ISqlAzureIdentityTokenProvider _tokenProvider;
        private static bool _connectionNeedsAccessToken = true;

        public SqlAzureIdentityAuthenticationDbConnectionInterceptor(ILogger<SqlAzureIdentityAuthenticationDbConnectionInterceptor> logger, ISqlAzureIdentityTokenProvider tokenProvider, bool connectionNeedsAccessToken)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connectionNeedsAccessToken = connectionNeedsAccessToken;
        }

        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            var sqlConnection = (SqlConnection)connection;
            if (_connectionNeedsAccessToken)
            {
                _logger.LogInformation("Getting AccessToken");
                var token = _tokenProvider.GetAccessToken();
                sqlConnection.AccessToken = token;
            }
            else
            {
                _logger.LogWarning($"Skipping GetAccessToken because ConnectionNeedsAccessToken is {_connectionNeedsAccessToken}");
            }

            return base.ConnectionOpening(connection, eventData, result);
        }

        public override async Task<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            var sqlConnection = (SqlConnection)connection;
            if (_connectionNeedsAccessToken)
            {
                _logger.LogInformation("Getting AccessToken Async");

                var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);
                
                sqlConnection.AccessToken = token;
            }
            else
            {
                _logger.LogWarning($"Skipping GetAccessToken Async because ConnectionNeedsAccessToken is {_connectionNeedsAccessToken}");
            }

            return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
        }
    }
}