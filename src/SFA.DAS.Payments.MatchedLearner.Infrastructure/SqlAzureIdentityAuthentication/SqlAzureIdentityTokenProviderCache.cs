using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.MatchedLearner.Infrastructure.SqlAzureIdentityAuthentication
{
    public class SqlAzureIdentityTokenProviderCache : ISqlAzureIdentityTokenProvider
    {
        private const string CacheKey = nameof(SqlAzureIdentityTokenProviderCache);
        private readonly ISqlAzureIdentityTokenProvider _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SqlAzureIdentityTokenProviderCache> _logger;
        
        public SqlAzureIdentityTokenProviderCache(
            ISqlAzureIdentityTokenProvider inner,
            IMemoryCache cache,
            ILogger<SqlAzureIdentityTokenProviderCache> logger)
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

                cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(50));

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
}