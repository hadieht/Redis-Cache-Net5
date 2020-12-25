namespace RedisCache
{
	public interface IRedisCacheClient
	{
		IRedisCache Db0 { get; }

		IRedisCache Db1 { get; }

		IRedisCache Db2 { get; }

		IRedisCache Db3 { get; }

		IRedisCache Db4 { get; }

		IRedisCache Db5 { get; }

		IRedisCache Db6 { get; }

		IRedisCache Db7 { get; }

		IRedisCache Db8 { get; }

		IRedisCache Db9 { get; }

		IRedisCache Db10 { get; }

		IRedisCache Db11 { get; }

		IRedisCache Db12 { get; }

		IRedisCache Db13 { get; }

		IRedisCache Db14 { get; }

		IRedisCache Db15 { get; }

		IRedisCache Db16 { get; }

		IRedisCache GetDb(int dbNumber, string keyPrefix = null);
		IRedisCache GetDbFromConfiguration();
	}
}
