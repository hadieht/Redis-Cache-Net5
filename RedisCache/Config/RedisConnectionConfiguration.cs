using RedisCache.Service;
using StackExchange.Redis;
using StackExchange.Redis.Profiling;
using System;
using System.Collections.Generic;
using System.Security.Authentication;

namespace RedisCache.Config
{
	public class RedisConnectionConfiguration
	{
		private ConfigurationOptions options;
		private string keyPrefix;
		private string password;
		private bool allowAdmin;
		private bool ssl;
		private int connectTimeout = 5000;
		private int syncTimeout = 1000;
		private bool abortOnConnectFail;
		private int database = 0;
		private RedisHostInfo[] hosts;
		private uint maxValueLength;
		private int poolSize = 5;
		private string[] excludeCommands;
		private string configurationChannel = null;
		private string connectionString = null;
		private string serviceName = null;
		private SslProtocols? sslProtocols = null;
		private Func<ProfilingSession> profilingSessionProvider;

		public string ServiceName
		{
			get => serviceName;
			set
			{
				serviceName = value;
				ResetConfigurationOptions();
			}
		}

		public bool IsSentinelCluster => !string.IsNullOrEmpty(ServiceName);

		public SslProtocols? SslProtocols
		{
			get => sslProtocols;
			set
			{
				sslProtocols = value;
				ResetConfigurationOptions();
			}
		}

		public string ConnectionString
		{
			get => connectionString;
			set
			{
				connectionString = value;
				ResetConfigurationOptions();
			}
		}

		public string ConfigurationChannel
		{
			get => configurationChannel;
			set
			{
				configurationChannel = value;
				ResetConfigurationOptions();
			}
		}

		public string KeyPrefix
		{
			get => keyPrefix;
			set
			{
				keyPrefix = value;
				ResetConfigurationOptions();
			}
		}

		public string Password
		{
			get => password;
			set
			{
				password = value;
				ResetConfigurationOptions();
			}
		}

		public bool AllowAdmin
		{
			get => allowAdmin;
			set
			{
				allowAdmin = value;
				ResetConfigurationOptions();
			}
		}

		public bool Ssl
		{
			get => ssl;
			set
			{
				ssl = value;
				ResetConfigurationOptions();
			}
		}

		public int ConnectTimeout
		{
			get => connectTimeout;
			set
			{
				connectTimeout = value;
				ResetConfigurationOptions();
			}
		}

		public int SyncTimeout
		{
			get => syncTimeout;
			set
			{
				syncTimeout = value;
				ResetConfigurationOptions();
			}
		}

		public bool AbortOnConnectFail
		{
			get => abortOnConnectFail;
			set
			{
				abortOnConnectFail = value;
				ResetConfigurationOptions();
			}
		}

		public int Database
		{
			get => database;
			set
			{
				database = value;
				ResetConfigurationOptions();
			}
		}


		public RedisHostInfo[] Hosts
		{
			get => hosts;
			set
			{
				hosts = value;
				ResetConfigurationOptions();
			}
		}

		public uint MaxValueLength
		{
			get => maxValueLength;
			set
			{
				maxValueLength = value;
				ResetConfigurationOptions();
			}
		}

		public int PoolSize
		{
			get => poolSize;
			set
			{
				poolSize = value;
				ResetConfigurationOptions();
			}
		}

		public string[] ExcludeCommands
		{
			get => excludeCommands;
			set
			{
				excludeCommands = value;
				ResetConfigurationOptions();
			}
		}

		public Func<ProfilingSession> ProfilingSessionProvider
		{
			get => profilingSessionProvider;
			set
			{
				profilingSessionProvider = value;
				ResetConfigurationOptions();
			}
		}

	 
		public StateAwareConnectionResolver StateAwareConnectionFactory { get; set; } = (cm, logger) => new RedisCacheConnection.RedisConnection(cm, logger);

		public ConfigurationOptions ConfigurationOptions
		{
			get
			{
				if (options == null)
				{
					ConfigurationOptions newOptions;

					if (!string.IsNullOrEmpty(ConnectionString))
					{
						newOptions = ConfigurationOptions.Parse(ConnectionString);
					}
					else
					{
						newOptions = new ConfigurationOptions
						{
							Ssl = Ssl,
							AllowAdmin = AllowAdmin,
							Password = Password,
							ConnectTimeout = ConnectTimeout,
							SyncTimeout = SyncTimeout,
							AbortOnConnectFail = AbortOnConnectFail,
							ConfigurationChannel = ConfigurationChannel,
							SslProtocols = sslProtocols,
							ChannelPrefix = KeyPrefix,
						};

						if (IsSentinelCluster)
						{
							newOptions.ServiceName = ServiceName;
							newOptions.CommandMap = CommandMap.Sentinel;
						}

						foreach (var redisHost in Hosts)
							newOptions.EndPoints.Add(redisHost.Host, redisHost.Port);
					}

					if (ExcludeCommands != null)
					{
						newOptions.CommandMap = CommandMap.Create(
								new HashSet<string>(ExcludeCommands),
								available: false);
					}

					newOptions.CertificateValidation += null;

					options = newOptions;
				}

				return options;
			}
		}

		private void ResetConfigurationOptions()
		{
			options = null;
		}
	}
}
