﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Cache;
using NHibernate.Caches.Common.Tests;
using NUnit.Framework;

namespace NHibernate.Caches.StackExRedis.Tests
{
	[TestFixture]
	public class RedisCacheFastStrategyFixture : RedisCacheFixture
	{
		protected override bool SupportsClear => false;

		protected override void Configure(Dictionary<string, string> defaultProperties)
		{
			base.Configure(defaultProperties);
			defaultProperties.Add(RedisEnvironment.RegionStrategy, typeof(FastRegionStrategy).AssemblyQualifiedName);
		}

		[Test]
		public void TestRegionStrategyType()
		{
			var cache = (RedisCache)GetDefaultCache();
			Assert.That(cache, Is.Not.Null, "cache is not a redis cache.");

			Assert.That(cache.RegionStrategy, Is.TypeOf<FastRegionStrategy>(), "cache strategy is not type of FastRegionStrategy");
		}

	}
}
