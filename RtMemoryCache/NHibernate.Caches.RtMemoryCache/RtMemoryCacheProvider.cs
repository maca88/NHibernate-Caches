#region License

//
//  RtMemoryCache - A cache provider for NHibernate using System.Runtime.Caching.MemoryCache.
//
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

#endregion

using System.Collections.Generic;
using System.Configuration;
using System.Text;
using NHibernate.Cache;

namespace NHibernate.Caches.RtMemoryCache
{
	/// <summary>
	/// Cache provider using the System.Runtime.Caching classes
	/// </summary>
	public class RtMemoryCacheProvider : ICacheProvider
	{
		private static readonly Dictionary<string, IDictionary<string, string>> ConfiguredCachesProperties;
		private static readonly IInternalLogger Log;

		static RtMemoryCacheProvider()
		{
			Log = LoggerProvider.LoggerFor(typeof(RtMemoryCacheProvider));
			ConfiguredCachesProperties = new Dictionary<string, IDictionary<string, string>>();

			if (!(ConfigurationManager.GetSection("rtmemorycache") is CacheConfig[] list))
				return;
			foreach (var cache in list)
			{
				ConfiguredCachesProperties.Add(cache.Region, cache.Properties);
			}
		}

		#region ICacheProvider Members

		/// <inheritdoc />
		public ICache BuildCache(string regionName, IDictionary<string, string> properties)
		{
			if (regionName == null)
			{
				regionName = string.Empty;
			}

			if (ConfiguredCachesProperties.TryGetValue(regionName, out var configuredProperties) && configuredProperties.Count > 0)
			{
				if (properties != null)
				{
					// Duplicate it for not altering the global configuration
					properties = new Dictionary<string, string>(properties);
					foreach (var prop in configuredProperties)
					{
						properties[prop.Key] = prop.Value;
					}
				}
				else
				{
					properties = configuredProperties;
				}
			}

			// create cache
			if (properties == null)
			{
				properties = new Dictionary<string, string>(1);
			}

			if (Log.IsDebugEnabled)
			{
				var sb = new StringBuilder();

				foreach (var de in properties)
				{
					sb.Append("name=");
					sb.Append(de.Key);
					sb.Append("&value=");
					sb.Append(de.Value);
					sb.Append(";");
				}

				Log.DebugFormat("building cache with region: {0}, properties: {1}" , regionName, sb.ToString());
			}
			return CreateCache(regionName, properties);
		}

		protected virtual ICache CreateCache(string regionName, IDictionary<string, string> properties)
		{
			return new RtMemoryCache(regionName, properties);
		}

		/// <inheritdoc />
		public long NextTimestamp()
		{
			return Timestamper.Next();
		}

		/// <inheritdoc />
		public void Start(IDictionary<string, string> properties)
		{
		}

		/// <inheritdoc />
		public void Stop()
		{
		}

		#endregion
	}
}
