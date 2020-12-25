using Microsoft.Extensions.DependencyInjection;
using RedisCache;
using RedisCache.Config;
using RedisCache.Service;
using System;

namespace Redis.Api.Cache.Sample
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddStackExchangeRedisExtensions(
																		this IServiceCollection services,
																		Func<IServiceProvider, RedisConnectionConfiguration> redisConfigurationFactory)

		{
			services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
			services.AddSingleton<IRedisCacheConnection, RedisCacheConnection>();

			services.AddSingleton((provider) =>
			{
				return provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration();
			});

			services.AddSingleton(redisConfigurationFactory);

			return services;
		}
	}
}
