using Hangfire;
using Hangfire.MemoryStorage;
using Xunit;

namespace Ecommerce.UnitTests.Fixtures
{
    public class HangfireFixture
    {
        private static readonly object _lock = new object();
        private static bool _initialized = false;

        public HangfireFixture()
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    GlobalConfiguration.Configuration.UseMemoryStorage();
                    _initialized = true;
                }
            }
        }
    }

    [CollectionDefinition("Hangfire")]
    public class HangfireCollection : ICollectionFixture<HangfireFixture>
    {
    }
}
