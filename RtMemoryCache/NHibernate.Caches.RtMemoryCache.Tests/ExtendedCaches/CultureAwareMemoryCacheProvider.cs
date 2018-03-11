using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Cache;

namespace NHibernate.Caches.RtMemoryCache.Tests.ExtendedCaches
{
	public class CultureAwareMemoryCacheProvider : RtMemoryCacheProvider
	{
		protected override ICache CreateCache(string regionName, IDictionary<string, string> properties)
		{
			return new CultureAwareMemoryCache(regionName, properties);
		}
	}
}
