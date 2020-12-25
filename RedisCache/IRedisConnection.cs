using StackExchange.Redis;
using System;

namespace RedisCache
{
	public interface IRedisConnection : IDisposable
	{
		IConnectionMultiplexer Connection { get; }
		bool IsConnected();

		long TotalOutstanding();
	}
}
