using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NHibernate.Caches.StackExRedis.Tests
{
	[TestFixture, Explicit]
	public class JsonSerializerPerformanceFixture : RedisCachePerformanceFixture
	{
		protected override void Configure(Dictionary<string, string> defaultProperties)
		{
			defaultProperties[RedisEnvironment.Serializer] = typeof(JsonRedisSerializer).AssemblyQualifiedName;
			base.Configure(defaultProperties);
		}
	}
}
