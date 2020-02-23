using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernate.Caches.StackExchangeRedis
{
	/// <summary>
	/// 
	/// </summary>
	public class ResilientConnectionConfiguration
	{
		/// <summary>
		/// 
		/// </summary>
		public TimeSpan ReconnectMinFrequency { get; set; } = TimeSpan.FromSeconds(60);

		/// <summary>
		/// 
		/// </summary>
		public TimeSpan ReconnectErrorThreshold { get; set; } = TimeSpan.FromSeconds(30);
	}
}
