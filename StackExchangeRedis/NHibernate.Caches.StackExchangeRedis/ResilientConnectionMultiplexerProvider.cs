using System.Collections.Generic;
using System.IO;
using StackExchange.Redis;
using static NHibernate.Caches.StackExchangeRedis.ConfigurationHelper;

namespace NHibernate.Caches.StackExchangeRedis
{
	/// <summary>
	/// TODO
	/// </summary>
	public class ResilientConnectionMultiplexerProvider : IConnectionMultiplexerProvider, IConnectionMultiplexerProviderExtended
	{
		private static readonly INHibernateLogger Log = NHibernateLogger.For(typeof(ResilientConnectionMultiplexerProvider));

		/// <inheritdoc />
		IConnectionMultiplexer IConnectionMultiplexerProvider.Get(string configuration)
		{
			return Get(configuration, new Dictionary<string, string>());
		}

		/// <inheritdoc />
		public IConnectionMultiplexer Get(string configuration, IDictionary<string, string> properties)
		{
			return new ResilientConnectionMultiplexer(
				() => ConnectionMultiplexer.Connect(configuration, CreateTextWriter()),
				() => ConnectionMultiplexer.ConnectAsync(configuration, CreateTextWriter()),
				GetConfiguration(properties)
			);
		}

		/// <summary>
		/// Creates a <see cref="TextWriter"/> used for the creation of <see cref="ConnectionMultiplexer"/>.
		/// </summary>
		protected virtual TextWriter CreateTextWriter()
		{
			return Log.IsDebugEnabled() ? new NHibernateTextWriter(Log) : null;
		}

		/// <summary>
		/// Gets the configuration that will be passed to <see cref="ResilientConnectionMultiplexer"/>.
		/// </summary>
		/// <param name="properties">NHibernate configuration settings.</param>
		protected virtual ResilientConnectionConfiguration GetConfiguration(IDictionary<string, string> properties)
		{
			var resilientConfiguration = new ResilientConnectionConfiguration();
			resilientConfiguration.ReconnectMinFrequency = GetTimeSpanFromSeconds(
				"cache.connection_multiplexer_provider.resilient.reconnect_min_frequency",
				properties,
				resilientConfiguration.ReconnectMinFrequency);
			resilientConfiguration.ReconnectErrorThreshold = GetTimeSpanFromSeconds(
				"cache.connection_multiplexer_provider.resilient.reconnect_error_threshold",
				properties,
				resilientConfiguration.ReconnectErrorThreshold);

			return resilientConfiguration;
		}
	}
}
