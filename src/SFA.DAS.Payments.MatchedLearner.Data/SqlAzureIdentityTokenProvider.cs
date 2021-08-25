using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.MatchedLearner.Data
{
    public interface ISqlAzureIdentityTokenProvider
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
        string GetAccessToken();
    }

    public class SqlAzureIdentityTokenProvider : ISqlAzureIdentityTokenProvider
    {
        private readonly ILogger<SqlAzureIdentityTokenProvider> _logger;
        private static readonly Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider AzureServiceTokenProvider = new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider();

        public SqlAzureIdentityTokenProvider(ILogger<SqlAzureIdentityTokenProvider> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var token = await AzureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/", cancellationToken: cancellationToken);

            _logger.LogInformation("Generated SQL AccessToken");

            return token;
        }

        public string GetAccessToken()
        {
            var token = AzureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult();
            
            _logger.LogInformation("Generated SQL AccessToken");

            return token;
        }
    }

    public class CacheSqlAzureIdentityTokenProvider : ISqlAzureIdentityTokenProvider
    {
        private const string CacheKey = nameof(CacheSqlAzureIdentityTokenProvider);
        private readonly ISqlAzureIdentityTokenProvider _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheSqlAzureIdentityTokenProvider> _logger;

        public CacheSqlAzureIdentityTokenProvider(
            ISqlAzureIdentityTokenProvider inner,
            IMemoryCache cache,
            ILogger<CacheSqlAzureIdentityTokenProvider> logger)
        {
            _inner = inner;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            return await _cache.GetOrCreateAsync(CacheKey, async cacheEntry =>
            {
                _logger.LogInformation("Cached AccessToken Expired");

                var token = await _inner.GetAccessTokenAsync(cancellationToken);

                cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddHours(1));

                _logger.LogInformation("Caching SQL Access Token for an hour");

                return token;
            });
        }

        public string GetAccessToken()
        {
            return _cache.GetOrCreate(CacheKey, cacheEntry =>
            {
                _logger.LogInformation("Cached AccessToken Expired, Generating new Token");

                var token = _inner.GetAccessToken();

                cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(50));

                _logger.LogInformation("New SQL Access Token Generated, Caching for an hour");

                return token;
            });
        }
    }

    public class SqlAzureIdentityAuthenticationDbConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly ISqlAzureIdentityTokenProvider _tokenProvider;

        public SqlAzureIdentityAuthenticationDbConnectionInterceptor(ISqlAzureIdentityTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            var sqlConnection = (SqlConnection)connection;
            if (ConnectionNeedsAccessToken(sqlConnection))
            {
                var token = _tokenProvider.GetAccessToken();
                sqlConnection.AccessToken = token;
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
            if (ConnectionNeedsAccessToken(sqlConnection))
            {
                var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);
                sqlConnection.AccessToken = token;
            }

            return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
        }

        private static bool ConnectionNeedsAccessToken(SqlConnection connection)
        {
            //
            // Only try to get a token from AAD if
            //  - We connect to an Azure SQL instance; and
            //  - The connection doesn't specify a username.
            //
            var connectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);

            return connectionStringBuilder.DataSource.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(connectionStringBuilder.UserID);
        }
    }
}