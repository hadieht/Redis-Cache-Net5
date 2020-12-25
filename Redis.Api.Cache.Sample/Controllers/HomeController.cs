using Microsoft.AspNetCore.Mvc;
using RedisCache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.Api.Cache.Sample.Controllers
{
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


		[HttpGet]
		[Route("getsetType")]
		public async Task<IActionResult> GetSetObjectCache()
		{
			var key = "pepole";
			var student = new Pepole
			{
				Id = 1,
				Name = "hd",
				ContactDetails = new ContactDetails
				{
					Email = "a@b.com",
					Phone = "123456"
				}
			};
			await redisCacheClient.GetDbFromConfiguration().AddAsync<Pepole>(key, student, DateTimeOffset.Now.AddMinutes(5));
			return Ok(await redisCacheClient.GetDbFromConfiguration().GetAsync<Pepole>(key));
		}

		[HttpGet]
		[Route("getsetType")]
		public async Task<IActionResult> GetSetListCache()
		{
			var contact1 = new ContactDetails
			{
				Email = "a@b.com",
				Phone = "12345"
			};
			var contact2 = new ContactDetails
			{
				Email = "ab@b.com",
				Phone = "321654"
			};

			var itemsToCache = new List<Tuple<string, ContactDetails>>();
			itemsToCache.Add(new Tuple<string, ContactDetails>("contact_1", contact1));
			itemsToCache.Add(new Tuple<string, ContactDetails>("contact_2", contact2));

			await redisCacheClient.GetDbFromConfiguration().AddAllAsync(itemsToCache, DateTimeOffset.Now.AddHours(1));

			var dataFromCache = await redisCacheClient.GetDbFromConfiguration().GetAllAsync<ContactDetails>(new List<string> { "contact_1", "contact_2" });
			return Ok(dataFromCache);
		}



		[HttpGet]
		[Route("remove")]
		public async Task<IActionResult> Remove()
		{
			await redisCacheClient.GetDbFromConfiguration().RemoveAsync("test");
			return Ok();
		}


		[HttpGet]
		[Route("clear")]
		public async Task<IActionResult> Clear()
		{
			await redisCacheClient.GetDbFromConfiguration().FlushDbAsync();
			return Ok();
		}

	}

	public class Pepole
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public ContactDetails ContactDetails { get; set; }
	}

	public class ContactDetails
	{
		public string Email { get; set; }
		public string Phone { get; set; }
	}
}
