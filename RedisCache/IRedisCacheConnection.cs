using StackExchange.Redis;
using System;

namespace RedisCache
{
	public interface IRedisCacheConnection : IDisposable
	{
		IConnectionMultiplexer GetConnection();
	}
}
