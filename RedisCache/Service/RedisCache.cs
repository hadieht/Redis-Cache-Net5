using RedisCache.Config;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedisCache.Service
{
	public partial class RedisCache : IRedisCache
	{
		private readonly IRedisCacheConnection connectionManager;
		private readonly int dbNumber;

		public RedisCache(IRedisCacheConnection connectionPoolManager,
											int dbNumber)
		{
			this.connectionManager = connectionPoolManager ?? throw new ArgumentNullException(nameof(connectionPoolManager));
			this.dbNumber = dbNumber;
		}
		public IDatabase Database => connectionManager.GetConnection().GetDatabase(dbNumber);

		public Task<bool> ExistsAsync(string key, CommandFlags flags = CommandFlags.None)
		{
			return Database.KeyExistsAsync(key, flags);
		}

		public Task<bool> RemoveAsync(string key, CommandFlags flags = CommandFlags.None)
		{
			return Database.KeyDeleteAsync(key, flags);
		}

		public Task<long> RemoveAllAsync(IEnumerable<string> keys, CommandFlags flags = CommandFlags.None)
		{
			var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
			return Database.KeyDeleteAsync(redisKeys, flags);
		}

		public async Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
		{
			var valueBytes = await Database.StringGetAsync(key, flag).ConfigureAwait(false);

			if (!valueBytes.HasValue)
				return default;

			return JsonSerializer.Deserialize<T>(valueBytes);
		}

		public async Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
		{
			var result = await GetAsync<T>(key, flag).ConfigureAwait(false);

			if (!Equals(result, default(T)))
				await Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow)).ConfigureAwait(false);

			return result;
		}

		public async Task<T> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
		{
			var result = await GetAsync<T>(key, flag).ConfigureAwait(false);

			if (!Equals(result, default(T)))
				await Database.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);

			return result;
		}

		public Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
		{
			var entryBytes = JsonSerializer.SerializeToUtf8Bytes(value);

			return Database.StringSetAsync(key, entryBytes, null, when, flag);
		}

		public Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
		{
			var entryBytes = JsonSerializer.SerializeToUtf8Bytes(value);

			var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

			return Database.StringSetAsync(key, entryBytes, expiration, when, flag);
		}

		public Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
		{
			var entryBytes = JsonSerializer.SerializeToUtf8Bytes(value);

			return Database.StringSetAsync(key, entryBytes, expiresIn, when, flag);
		}

		public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys)
		{
			var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
			var result = await Database.StringGetAsync(redisKeys).ConfigureAwait(false);
			var dict = new Dictionary<string, T>(redisKeys.Length, StringComparer.Ordinal);

			for (var index = 0; index < redisKeys.Length; index++)
			{
				var value = result[index];
				dict.Add(redisKeys[index], value == RedisValue.Null ? default : JsonSerializer.Deserialize<T>(value));
			}

			return dict;
		}

		public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, DateTimeOffset expiresAt)
		{
			return await GetAllAsync<T>(keys).ConfigureAwait(false);
		}

		public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, TimeSpan expiresIn)
		{
			return await GetAllAsync<T>(keys).ConfigureAwait(false);
		}

		public Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None)
		{
			var values = items
									.ValueInListSize()
									.Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
									.ToArray();

			return Database.StringSetAsync(values, when, flag);
		}

		public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
		{
			var values = items
									.ValueInListSize()
									.Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
									.ToArray();

			var tasks = new Task[values.Length];
			await Database.StringSetAsync(values, when, flag);

			for (var i = 0; i < values.Length; i++)
				tasks[i] = Database.KeyExpireAsync(values[i].Key, expiresAt.UtcDateTime, flag);

			await Task.WhenAll(tasks).ConfigureAwait(false);

			return ((Task<bool>)tasks[0]).Result;
		}

		public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn, When when = When.Always, CommandFlags flag = CommandFlags.None)
		{
			var values = items
									.ValueInListSize()
									.Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
									.ToArray();

			var tasks = new Task[values.Length];
			await Database.StringSetAsync(values, when, flag);

			for (var i = 0; i < values.Length; i++)
				tasks[i] = Database.KeyExpireAsync(values[i].Key, expiresOn, flag);

			await Task.WhenAll(tasks).ConfigureAwait(false);

			return ((Task<bool>)tasks[0]).Result;
		}

		public Task FlushDbAsync()
		{
			var endPoints = Database.Multiplexer.GetEndPoints();

			var tasks = new List<Task>(endPoints.Length);

			for (var i = 0; i < endPoints.Length; i++)
			{
				var server = Database.Multiplexer.GetServer(endPoints[i]);

				if (!server.IsReplica)
					tasks.Add(server.FlushDatabaseAsync(Database.Database));
			}

			return Task.WhenAll(tasks);
		}

		public Task SaveAsync(SaveType saveType, CommandFlags flags = CommandFlags.None)
		{
			var endPoints = Database.Multiplexer.GetEndPoints();

			var tasks = new Task[endPoints.Length];

			for (var i = 0; i < endPoints.Length; i++)
				tasks[i] = Database.Multiplexer.GetServer(endPoints[i]).SaveAsync(saveType, flags);

			return Task.WhenAll(tasks);
		}

		public async Task<Dictionary<string, string>> GetInfoAsync()
		{
			var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')").ConfigureAwait(false)).ToString();

			return ParseInfo(info);
		}

		public async Task<List<InfoDetail>> GetInfoCategorizedAsync()
		{
			var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')").ConfigureAwait(false)).ToString();

			return ParseCategorizedInfo(info);
		}

		private Dictionary<string, string> ParseInfo(string info)
		{
			var data = ParseCategorizedInfo(info);

			return data.ToDictionary(x => x.Key, x => x.Value);
		}

		private List<InfoDetail> ParseCategorizedInfo(string info)
		{
			var data = new List<InfoDetail>();
			var category = string.Empty;
			if (!string.IsNullOrWhiteSpace(info))
			{
				var lines = info.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
				{
					if (line[0] == '#')
					{
						category = line.Replace("#", string.Empty).Trim();
						continue;
					}

					var idx = line.IndexOf(':');
					if (idx > 0)
					{
						var key = line.Substring(0, idx);
						var infoValue = line.Substring(idx + 1).Trim();

						data.Add(new InfoDetail { Category = category, Key = key, Value = infoValue });
					}
				}
			}

			return data;
		}

		public async Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flags = CommandFlags.None)
		{
			if (await Database.KeyExistsAsync(key).ConfigureAwait(false))
				return await Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow), flags).ConfigureAwait(false);

			return false;
		}

		public async Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flags = CommandFlags.None)
		{
			if (await Database.KeyExistsAsync(key).ConfigureAwait(false))
				return await Database.KeyExpireAsync(key, expiresIn, flags).ConfigureAwait(false);

			return false;
		}

		public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt, CommandFlags flags = CommandFlags.None)
		{
			var tasks = new Task<bool>[keys.Length];

			for (var i = 0; i < keys.Length; i++)
				tasks[i] = UpdateExpiryAsync(keys[i], expiresAt.UtcDateTime, flags);

			await Task.WhenAll(tasks).ConfigureAwait(false);

			var results = new Dictionary<string, bool>(keys.Length, StringComparer.Ordinal);

			for (var i = 0; i < keys.Length; i++)
				results.Add(keys[i], tasks[i].Result);

			return results;
		}

		public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn, CommandFlags flags = CommandFlags.None)
		{
			var tasks = new Task<bool>[keys.Length];

			for (var i = 0; i < keys.Length; i++)
				tasks[i] = UpdateExpiryAsync(keys[i], expiresIn, flags);

			await Task.WhenAll(tasks).ConfigureAwait(false);

			var results = new Dictionary<string, bool>(keys.Length, StringComparer.Ordinal);

			for (var i = 0; i < keys.Length; i++)
				results.Add(keys[i], tasks[i].Result);

			return results;
		}


	}
}
