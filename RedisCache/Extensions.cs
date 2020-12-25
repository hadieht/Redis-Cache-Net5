using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RedisCache
{
	internal static class Extensions
	{
		public static IEnumerable<KeyValuePair<string, byte[]>> ValueInListSize<T>(this IEnumerable<Tuple<string, T>> items)
		{
			using var iterator = items.GetEnumerator();

			while (iterator.MoveNext())
			{
				yield return new KeyValuePair<string, byte[]>(
						iterator.Current.Item1,
						JsonSerializer.SerializeToUtf8Bytes(iterator.Current.Item2));
			}
		}

	}
}
