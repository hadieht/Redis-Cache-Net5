using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RedisCache.Config
{
	public delegate IRedisConnection StateAwareConnectionResolver(IConnectionMultiplexer connectionMultiplexer, ILogger logger);
}
