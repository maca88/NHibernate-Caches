using System;
using System.Collections.Generic;
using System.IO;
using StackExchange.Redis;


namespace NHibernate.Caches.StackExchangeRedis
{
	/// <inheritdoc cref="IConnectionMultiplexerProviderExtended" />
	public class DefaultConnectionMultiplexerProvider : IConnectionMultiplexerProvider, IConnectionMultiplexerProviderExtended
	{
		private static readonly INHibernateLogger Log = NHibernateLogger.For(typeof(DefaultConnectionMultiplexerProvider));

		/// <inheritdoc />
		// Since 5.7
		[Obsolete("Use Get method with properties parameter instead.")]
		public IConnectionMultiplexer Get(string configuration)
		{
			return Get(configuration, new Dictionary<string, string>());
		}

		/// <inheritdoc />
		public IConnectionMultiplexer Get(string configuration, IDictionary<string, string> properties)
		{
			TextWriter textWriter = Log.IsDebugEnabled() ? new NHibernateTextWriter(Log) : null;
			var connectionMultiplexer = ConnectionMultiplexer.Connect(configuration, textWriter);
			return connectionMultiplexer;
		}
	}
}
