using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RedisCache.Config;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RedisCache.Service
{
	public class RedisCacheConnection : IRedisCacheConnection
	{
		private readonly ConcurrentBag<Lazy<IRedisConnection>> connections;
		private readonly RedisConnectionConfiguration redisConfiguration;
		private readonly ILogger<RedisCacheConnection> logger;

		public RedisCacheConnection(RedisConnectionConfiguration redisConfiguration, ILogger<RedisCacheConnection> logger = null)
		{
			this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));

			this.connections = new ConcurrentBag<Lazy<IRedisConnection>>();
			this.logger = logger ?? NullLogger<RedisCacheConnection>.Instance;
		}

		public void Dispose()
		{
			var activeConnections = this.connections.Where(l => l.IsValueCreated).ToList();

			foreach (var connection in activeConnections)
				connection.Value.Dispose();

			while (this.connections.IsEmpty == false)
				this.connections.TryTake(out var taken);
		}

		public IConnectionMultiplexer GetConnection()
		{
			this.EmitConnections();

			var loadedLazies = this.connections.Where(l => l.IsValueCreated);

			if (loadedLazies.Count() == this.connections.Count)
				return this.connections.OrderBy(x => x.Value.TotalOutstanding()).First().Value.Connection;

			return this.connections.First(l => !l.IsValueCreated).Value.Connection;
		}

		private void EmitConnection()
		{
			this.connections.Add(new Lazy<IRedisConnection>(() =>
			{
				this.logger.LogDebug("Creating new Redis connection.");

				var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);

				if (this.redisConfiguration.ProfilingSessionProvider != null)
					multiplexer.RegisterProfiler(this.redisConfiguration.ProfilingSessionProvider);

				return this.redisConfiguration.StateAwareConnectionFactory(multiplexer, logger);
			}));
		}

		private void EmitConnections()
		{
			if (connections.Count >= this.redisConfiguration.PoolSize)
				return;

			for (var i = 0; i < this.redisConfiguration.PoolSize; i++)
			{
				logger.LogDebug("Creating the redis connection pool with {0} connections.", this.redisConfiguration.PoolSize);
				this.EmitConnection();
			}
		}

		internal sealed class RedisConnection : IRedisConnection
		{
			private readonly ILogger logger;
			public RedisConnection(IConnectionMultiplexer multiplexer, ILogger logger)
			{
				this.Connection = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
				this.Connection.ConnectionFailed += this.ConnectionFailed;
				this.Connection.ConnectionRestored += this.ConnectionRestored;

				this.logger = logger;
			}

			public IConnectionMultiplexer Connection { get; private set; }

			public long TotalOutstanding() => this.Connection.GetCounters().TotalOutstanding;

			public bool IsConnected() => this.Connection.IsConnecting == false;

			public void Dispose()
			{
				this.Connection.ConnectionFailed -= ConnectionFailed;
				this.Connection.ConnectionRestored -= ConnectionRestored;

				Connection.Dispose();
			}

			private void ConnectionFailed(object sender, ConnectionFailedEventArgs e)
			{
				logger.LogError(e.Exception, "Redis connection error {0}.", e.FailureType);
			}

			private void ConnectionRestored(object sender, ConnectionFailedEventArgs e)
			{
				logger.LogError("Redis connection error restored.");
			}
		}
	}


}
