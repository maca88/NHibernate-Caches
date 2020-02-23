using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace NHibernate.Caches.StackExchangeRedis
{
	/// <summary>
	/// 
	/// </summary>
	public interface IResilientConnectionMultiplexer : IConnectionMultiplexer
	{
		/// <summary>
		/// An event that is triggered when the <see cref="IConnectionMultiplexer"/> is recreated.
		/// </summary>
		event EventHandler<IConnectionMultiplexer> Reconnected;

		/// <summary>
		/// Last reconnect occurrence which is set by <see cref="DateTimeOffset.UtcTicks"/> or zero when no reconnect occurred.
		/// </summary>
		long LastReconnectTicks { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		bool TryReconnect();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		Task<bool> TryReconnectAsync();
	}
}
