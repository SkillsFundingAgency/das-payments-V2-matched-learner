using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace SFA.DAS.Payments.MatchedLearner.Data
{
    // Simple interface that represents a token acquisition abstraction
    public interface ISqlAzureIdentityTokenProvider
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
        string GetAccessToken();
    }

    // Core implementation that performs token acquisition with Azure Identity
    public class SqlAzureIdentityTokenProvider : ISqlAzureIdentityTokenProvider
    {
        private static readonly Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider AzureServiceTokenProvider = new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider();

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
           var token = await AzureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/", cancellationToken: cancellationToken);

            return token;
        }

        public string GetAccessToken()
        {
            var token = AzureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult();

            return token;
        }
    }

    // Decorator that caches tokens in the in-memory cache
    public class CacheSqlAzureIdentityTokenProvider : ISqlAzureIdentityTokenProvider
    {
        private const string CacheKey = nameof(CacheSqlAzureIdentityTokenProvider);
        private readonly ISqlAzureIdentityTokenProvider _inner;
        private readonly IMemoryCache _cache;

        public CacheSqlAzureIdentityTokenProvider(
            ISqlAzureIdentityTokenProvider inner,
            IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            return await _cache.GetOrCreateAsync(CacheKey, async cacheEntry =>
            {
                var token = await _inner.GetAccessTokenAsync(cancellationToken);

                // AAD access tokens have a default lifetime of 1 hour, so we take a small safety margin
                cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(50));

                return token;
            });
        }

        public string GetAccessToken()
        {
            return _cache.GetOrCreate(CacheKey, cacheEntry =>
            {
                var token = _inner.GetAccessToken();

                // AAD access tokens have a default lifetime of 1 hour, so we take a small safety margin
                cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(50));

                return token;
            });
        }
    }

    // The interceptor is now using the token provider abstraction
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