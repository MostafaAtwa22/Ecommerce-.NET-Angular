using StackExchange.Redis;

namespace Ecommerce.Infrastructure.Repositories
{
    public class RedisRepository<T> : IRedisRepository<T> where T : class
    {
        private readonly IDatabase _database;
        private readonly IConfiguration _config;
        
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public RedisRepository(IConnectionMultiplexer redis,
            IConfiguration config)
        {
            _database = redis.GetDatabase();
            _config = config;
        }

        public async Task<T?> GetAsync(string id)
        {
            var data = await _database.StringGetAsync(id);

            return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(data!, _options);
        }

        public async Task<T?> UpdateOrCreateAsync(string id, T entity, TimeSpan? expiry = null)
        {
            var timeToLive = expiry ?? TimeSpan.FromDays(int.Parse(_config["Redis:DefaultTTL"]!));

            var created = await _database.StringSetAsync(
                id,
                JsonSerializer.Serialize<T>(entity, _options),
                timeToLive
            );

            return created ? entity : null;
        }

        public async Task<bool> DeleteAsync(string id)
            => await _database.KeyDeleteAsync(id);
    }
}
