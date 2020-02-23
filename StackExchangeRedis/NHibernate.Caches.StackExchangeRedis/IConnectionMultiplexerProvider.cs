using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace NHibernate.Caches.StackExchangeRedis
{
	/// <summary>
	/// Defines a method to provide an <see cref="IConnectionMultiplexer"/> instance.
	/// </summary>
	public interface IConnectionMultiplexerProvider
	{
		/// <summary>
		/// Provide the <see cref="IConnectionMultiplexer"/> for the StackExchange.Redis configuration string.
		/// </summary>
		/// <param name="configuration">The StackExchange.Redis configuration string</param>
		/// <returns>The <see cref="IConnectionMultiplexer"/> instance.</returns>
		// Since 5.7
		[Obsolete("Use Get extension method with properties parameter instead.")]
		IConnectionMultiplexer Get(string configuration);
	}

	// TODO 6.0 : Move to IConnectionMultiplexerProvider
	internal interface IConnectionMultiplexerProviderExtended : IConnectionMultiplexerProvider
	{
		/// <summary>
		/// Provide the <see cref="IConnectionMultiplexer"/> for the StackExchange.Redis configuration string.
		/// </summary>
		/// <param name="configuration">The StackExchange.Redis configuration string</param>
		/// <param name="properties">NHibernate configuration settings.</param>
		/// <returns>The <see cref="IConnectionMultiplexer"/> instance.</returns>
		IConnectionMultiplexer Get(string configuration, IDictionary<string, string> properties);
	}

	/// <summary>
	/// Extension methods for <see cref="IConnectionMultiplexerProvider"/>.
	/// </summary>
	public static class ConnectionMultiplexerProviderExtensions
	{
		/// <summary>
		/// Provide the <see cref="IConnectionMultiplexer"/> for the StackExchange.Redis configuration string.
		/// </summary>
		/// <param name="multiplexerProvider">The multiplexer provider.</param>
		/// <param name="configuration">The StackExchange.Redis configuration string</param>
		/// <param name="properties">NHibernate configuration settings.</param>
		/// <returns>The <see cref="IConnectionMultiplexer"/> instance.</returns>
		public static IConnectionMultiplexer Get(this IConnectionMultiplexerProvider multiplexerProvider, string configuration, IDictionary<string, string> properties)
		{
			if (multiplexerProvider is IConnectionMultiplexerProviderExtended multiplexerProviderExtended)
			{
				return multiplexerProviderExtended.Get(configuration, properties);
			}

#pragma warning disable CS0618
			return multiplexerProvider.Get(configuration);
#pragma warning restore CS0618
		}
	}
}
