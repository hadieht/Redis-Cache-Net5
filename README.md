# Redis-Cache-Net5
Redis Cache library with simple usage in .Net 5

# What is this ?

Simple library to Using [Redis](http://redis.io) Cache In .NET Core Projects
You can use this Library in Microservice Architecture  with Client-Server Pattern for Cache.
also you can assign db0, db1, for each Microservice  

# How to use ?

First you must install redis server. in quickly way you can install chocolaty and [install redis](https://chocolatey.org/packages/redis-64/) through it. then run following command in cmd (run as administrator);

```code
C:\> redis-server 
```


Then :

1- add required services to Startup class as below :
```code
      services.AddStackExchangeRedisExtensions(option =>
			{
				return Configuration.GetSection("Redis").Get<RedisConnectionConfiguration>();
			});
```
3- Use IRedisCacheService in your app :
```code
  [ApiController]
	[Route("cache")]
	public class HomeController : ControllerBase
	{
		private readonly IRedisCacheClient redisCacheClient;

		public HomeController(IRedisCacheClient redisCacheClient)
		{
			this.redisCacheClient = redisCacheClient;
		}

		[Route("getset")]
		[HttpGet]
		public async Task<IActionResult> SimpleGetSet()
		{
			await redisCacheClient.GetDbFromConfiguration().AddAsync<string>("mynames", "hadi", DateTimeOffset.Now.AddMinutes(10));

			var value = await redisCacheClient.GetDbFromConfiguration().GetAsync<string>("mynames");

			return Ok(value);

		}
   }

```

