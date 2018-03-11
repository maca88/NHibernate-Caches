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
// CLOVER:OFF
//

#endregion

using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using NHibernate.Caches.RtMemoryCache.Tests.ExtendedCaches;
using NUnit.Framework;

namespace NHibernate.Caches.RtMemoryCache.Tests
{
	[TestFixture]
	public partial class CultureAwareMemoryCacheFixture
	{
		private readonly CultureInfo _enCulture = CultureInfo.GetCultureInfo("en");
		private readonly CultureInfo _itCulture = CultureInfo.GetCultureInfo("it");

		[Test]
		public void DictionaryValueTest()
		{
			var cache = new CultureAwareMemoryCache("CultureAware(Name)", null);
			var oldCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = _enCulture;
			const string key = "Status";
			var valueEn = new Hashtable(3)
			{
				{"Id", 1},
				{"Name", "Ready"},
				{"Active", true}
			};
			var valueIt = new Hashtable(3)
			{
				{"Id", 1},
				{"Name", "Pronto"},
				{"Active", true}
			};

			cache.Put(key, valueEn.Clone());
			var item = cache.Get(key);
			Assert.That(item,  Is.EquivalentTo(valueEn));

			Thread.CurrentThread.CurrentCulture = _itCulture;
			item = cache.Get(key);
			Assert.That(item, Is.Null);

			cache.Put(key, valueIt.Clone());
			item = cache.Get(key);
			Assert.That(item, Is.EquivalentTo(valueIt));

			Thread.CurrentThread.CurrentCulture = _enCulture;
			item = cache.Get(key);
			Assert.That(item, Is.EquivalentTo(valueEn));

			// Putting the same key with the same culture twice, should not invalidate
			// other cultures if none of the culture non-aware properties changed.
			cache.Put(key, valueEn.Clone());
			Thread.CurrentThread.CurrentCulture = _itCulture;
			item = cache.Get(key);
			Assert.That(item, Is.EquivalentTo(valueIt));

			// Changing one culture non-aware property should invalidate other cultures
			item = valueIt.Clone();
			((Hashtable) item)["Active"] = false;
			cache.Put(key, item);
			Thread.CurrentThread.CurrentCulture = _enCulture;
			item = cache.Get(key);
			Assert.That(item, Is.Null);

			// One culture should invalidate other cultures when a culture non-aware 
			// property is changed
			cache.Put(key, valueEn.Clone()); // Reset Active to true
			Thread.CurrentThread.CurrentCulture = _itCulture;
			item = cache.Get(key);
			Assert.That(item, Is.Null);

			Thread.CurrentThread.CurrentCulture = oldCulture;
		}
	}
}
