using RedisCache.Config;

namespace RedisCache.Service
{
	public class RedisCacheClient : IRedisCacheClient
	{
		private readonly IRedisCacheConnection connectionPoolManager;
		private readonly RedisConnectionConfiguration redisConfiguration;

		public RedisCacheClient(IRedisCacheConnection connectionPoolManager,
														RedisConnectionConfiguration redisConfiguration)
		{
			this.connectionPoolManager = connectionPoolManager;
			this.redisConfiguration = redisConfiguration;
		}

		public IRedisCache Db0 => GetDb(0);
		public IRedisCache Db1 => GetDb(1);
		public IRedisCache Db2 => GetDb(2);
		public IRedisCache Db3 => GetDb(3);
		public IRedisCache Db4 => GetDb(4);
		public IRedisCache Db5 => GetDb(5);
		public IRedisCache Db6 => GetDb(6);
		public IRedisCache Db7 => GetDb(7);
		public IRedisCache Db8 => GetDb(8);
		public IRedisCache Db9 => GetDb(9);
		public IRedisCache Db10 => GetDb(10);
		public IRedisCache Db11 => GetDb(11);
		public IRedisCache Db12 => GetDb(12);
		public IRedisCache Db13 => GetDb(13);
		public IRedisCache Db14 => GetDb(14);
		public IRedisCache Db15 => GetDb(15);
		public IRedisCache Db16 => GetDb(16);

		public IRedisCache GetDb(int dbNumber, string keyPrefix = null)
		{
			return new RedisCache(connectionPoolManager, dbNumber);
		}

		public IRedisCache GetDbFromConfiguration()
		{
			return GetDb(redisConfiguration.Database, redisConfiguration.KeyPrefix);
		}
	}
}
