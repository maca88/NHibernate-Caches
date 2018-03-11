using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;

namespace NHibernate.Caches.RtMemoryCache.Tests.ExtendedCaches
{
	public class CultureAwareMemoryCache : RtMemoryCache
	{
		private static readonly Regex CultureAwareRegex = new Regex(@"CultureAware\(([\w,]+)\)", RegexOptions.Compiled);
		private readonly bool _cultureAware;
		private readonly HashSet<string> _cultureAwareProperties;

		public CultureAwareMemoryCache(string region, IDictionary<string, string> properties) : base(region, properties)
		{
			var match = CultureAwareRegex.Match(Region);
			_cultureAware = match.Success;
			if (_cultureAware)
			{
				_cultureAwareProperties = new HashSet<string>(match.Groups[1].Value.Split(','));
			}
		}

		protected override string GetCacheKey(object key)
		{
			return  _cultureAware
				? string.Concat(base.GetCacheKey(key), ":", Thread.CurrentThread.CurrentCulture.Name)
				: base.GetCacheKey(key);
		}

		public override void Put(object key, object value)
		{
			if (!_cultureAware)
			{
				base.Put(key, value);
				return;
			}
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key), "null key not allowed");
			}
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value), "null value not allowed");
			}
			var cacheKey = GetCacheKey(key);
			var groupCacheKey = base.GetCacheKey(key);
			var groupCacheValue = Cache.Get(groupCacheKey);
			var removeGroup = groupCacheValue != null && ShouldResetGroup(groupCacheValue, value);
			if (removeGroup)
			{
				Cache.Remove(groupCacheKey);
			}
			if (groupCacheValue == null || removeGroup)
			{
				Cache.Add(groupCacheKey, value,
					new CacheItemPolicy
					{
						AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration,
						SlidingExpiration = ObjectCache.NoSlidingExpiration,
						ChangeMonitors = { Cache.CreateCacheEntryChangeMonitor(new[] { RootCacheKey }) }
					});
			}

			StoreRootCacheKey();

			if (Cache[cacheKey] != null)
			{
				// Remove the key to re-add it again below
				Cache.Remove(cacheKey);
			}
			Cache.Add(cacheKey, new DictionaryEntry(key, value),
				new CacheItemPolicy
				{
					AbsoluteExpiration = UseSlidingExpiration ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.UtcNow.Add(Expiration),
					SlidingExpiration = UseSlidingExpiration ? Expiration : ObjectCache.NoSlidingExpiration,
					ChangeMonitors = { Cache.CreateCacheEntryChangeMonitor(new[] { groupCacheKey, RootCacheKey }) }
				});
		}

		private bool ShouldResetGroup(object oldValue, object newValue)
		{
			if (_cultureAwareProperties == null)
			{
				return true;
			}
			if (!(oldValue is IDictionary oldDict) || !(newValue is IDictionary newDict))
			{
				return true;
			}
			foreach (DictionaryEntry pair in oldDict)
			{
				if (_cultureAwareProperties.Contains(pair.Key.ToString()))
				{
					continue;
				}
				if (!Equals(newDict[pair.Key], pair.Value))
				{
					return true;
				}
			}
			return false;
		}
	}
}
