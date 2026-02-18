using StackExchange.Redis;
using System.Text.Json;

namespace Ecommerce.UnitTests.ServiceTests
{
    public class ResponseCacheServiceTests
    {
        private readonly Mock<IConnectionMultiplexer> _redis;
        private readonly Mock<IDatabase> _database;
        private readonly ResponseCacheService _service;

        public ResponseCacheServiceTests()
        {
            _redis = new Mock<IConnectionMultiplexer>();
            _database = new Mock<IDatabase>();

            _redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                  .Returns(_database.Object);

            _service = new ResponseCacheService(_redis.Object);
        }

        [Fact]
        public async Task CacheResponseAsync_WithNullResponse_Returns()
        {
            await _service.CacheResponseAsync("key", null!, TimeSpan.FromMinutes(1));
            
            _database.Verify(d => d.StringSetAsync(
                It.IsAny<RedisKey>(), 
                It.IsAny<RedisValue>(), 
                It.IsAny<TimeSpan?>(), 
                It.IsAny<bool>(), 
                It.IsAny<When>(), 
                It.IsAny<CommandFlags>()), Times.Never);
        }

        [Fact]
        public async Task CacheResponseAsync_WithValidResponse_CachesData()
        {
            var response = new { Id = 1, Name = "Test" };
            var timeToLive = TimeSpan.FromMinutes(1);
            
            await _service.CacheResponseAsync("key", response, timeToLive);

            _database.Verify(d => d.StringSetAsync(
                "key", 
                It.Is<RedisValue>(v => v.HasValue), 
                timeToLive, 
                false, 
                When.Always, 
                CommandFlags.None), Times.Once);
        }

        [Fact]
        public async Task GetCachedResponseAsync_KeyExists_ReturnsResponse()
        {
            var cachedValue = "{\"id\":1}";
            _database.Setup(d => d.StringGetAsync("key", CommandFlags.None))
                     .ReturnsAsync(cachedValue);

            var result = await _service.GetCachedResponseAsync("key");

            Assert.Equal(cachedValue, result);
        }

        [Fact]
        public async Task GetCachedResponseAsync_KeyDoesNotExist_ReturnsNull()
        {
            _database.Setup(d => d.StringGetAsync("key", CommandFlags.None))
                     .ReturnsAsync(RedisValue.Null);

            var result = await _service.GetCachedResponseAsync("key");

            Assert.Null(result);
        }


        [Fact]
        public async Task RemoveCacheByPatternAsync_WithMatchingKeys_DeletesKeys()
        {
            // Arrange
            var endpoints = new System.Net.EndPoint[] { new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 6379) };
            var server = new Mock<IServer>();
            var keys = new RedisKey[] { "key1", "key2" };

            _redis.Setup(r => r.GetEndPoints(false)).Returns(endpoints);
            _redis.Setup(r => r.GetServer(It.IsAny<System.Net.EndPoint>(), null)).Returns(server.Object);
            server.Setup(s => s.Keys(It.IsAny<int>(), "*pattern*", It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>())).Returns(keys);

            // Act
            await _service.RemoveCacheByPatternAsync("pattern");

            // Assert
            _database.Verify(d => d.KeyDeleteAsync(keys, CommandFlags.None), Times.Once);
        }

        [Fact]
        public async Task RemoveCacheByPatternAsync_NoMatchingKeys_DoesNothing()
        {
            // Arrange
            var endpoints = new System.Net.EndPoint[] { new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 6379) };
            var server = new Mock<IServer>();
            var keys = Array.Empty<RedisKey>();

            _redis.Setup(r => r.GetEndPoints(false)).Returns(endpoints);
            _redis.Setup(r => r.GetServer(It.IsAny<System.Net.EndPoint>(), null)).Returns(server.Object);
            server.Setup(s => s.Keys(It.IsAny<int>(), "*pattern*", It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>())).Returns(keys);

            // Act
            await _service.RemoveCacheByPatternAsync("pattern");

            // Assert
            _database.Verify(d => d.KeyDeleteAsync(It.IsAny<RedisKey[]>(), CommandFlags.None), Times.Never);
        }
    }
}
