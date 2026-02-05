using System.Text.Json;
using Ecommerce.Core.Interfaces;
using StackExchange.Redis;

namespace Ecommerce.Infrastructure.Services
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public ResponseCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
        {
            if (response is null)
                return;
            var option = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var serialisedResponse = JsonSerializer.Serialize(response, option);

            await _database.StringSetAsync(cacheKey, serialisedResponse, timeToLive);
        }

        public async Task<string> GetCachedResponseAsync(string cacheKey)
        {
            var cacheReponse = await _database.StringGetAsync(cacheKey);

            if (cacheReponse.IsNullOrEmpty)
                return null!;

            return cacheReponse!;
        }

        public async Task RemoveCacheByPatternAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"*{pattern}*").ToArray();

            if (keys.Length > 0)
                await _database.KeyDeleteAsync(keys);
        }
    }
}