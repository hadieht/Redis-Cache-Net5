using RedisCache.Config;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisCache
{
	public  interface IRedisCache
	{
		IDatabase Database { get; }
		Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None);
		Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None);
		Task<long> RemoveAllAsync(IEnumerable<string> keys, CommandFlags flag = CommandFlags.None);
		Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None);
		Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);
		Task<T> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);
		Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);
		Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);
		Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);
		Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys);
		Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, DateTimeOffset expiresAt);
		Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, TimeSpan expiresIn);
		Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);
		Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None);
		Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);
		Task FlushDbAsync();
		Task SaveAsync(SaveType saveType, CommandFlags flag = CommandFlags.None);
		Task<Dictionary<string, string>> GetInfoAsync();
		Task<List<InfoDetail>> GetInfoCategorizedAsync();
		Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);
		Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);
		Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);
		Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);

	}
}
