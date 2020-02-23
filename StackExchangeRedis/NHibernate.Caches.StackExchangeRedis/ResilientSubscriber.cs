using System;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace NHibernate.Caches.StackExchangeRedis
{
	internal class ResilientSubscriber : ISubscriber
	{
		private readonly IResilientConnectionMultiplexer _resilientConnectionMultiplexer;
		private readonly Func<ISubscriber> _subscriberProvider;
		private readonly object _resetLock = new object();
		private AtomicLazy<ISubscriber> _subscriber;
		private long _lastReconnectTicks;

		public ResilientSubscriber(IResilientConnectionMultiplexer resilientConnectionMultiplexer, Func<ISubscriber> subscriberProvider)
		{
			_resilientConnectionMultiplexer = resilientConnectionMultiplexer;
			_subscriberProvider = subscriberProvider;
			_lastReconnectTicks = resilientConnectionMultiplexer.LastReconnectTicks;
			ResetSubscriber();
		}

		#region ISubscriber implementation

		/// <inheritdoc />
		public IConnectionMultiplexer Multiplexer => _resilientConnectionMultiplexer;

		/// <inheritdoc />
		public EndPoint IdentifyEndpoint(RedisChannel channel, CommandFlags flags = CommandFlags.None)
		{
			return ExecuteAction(() => _subscriber.Value.IdentifyEndpoint(channel, flags));
		}

		/// <inheritdoc />
		public Task<EndPoint> IdentifyEndpointAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None)
		{
			return ExecuteActionAsync(() => _subscriber.Value.IdentifyEndpointAsync(channel, flags));
		}

		/// <inheritdoc />
		public bool IsConnected(RedisChannel channel = default)
		{
			return ExecuteAction(() => _subscriber.Value.IsConnected(channel));
		}

		/// <inheritdoc />
		public TimeSpan Ping(CommandFlags flags = CommandFlags.None)
		{
			return ExecuteAction(() => _subscriber.Value.Ping(flags));
		}

		/// <inheritdoc />
		public Task<TimeSpan> PingAsync(CommandFlags flags = CommandFlags.None)
		{
			return ExecuteActionAsync(() => _subscriber.Value.PingAsync(flags));
		}

		/// <inheritdoc />
		public long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
		{
			return ExecuteAction(() => _subscriber.Value.Publish(channel, message, flags));
		}

		/// <inheritdoc />
		public Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
		{
			return ExecuteActionAsync(() => _subscriber.Value.PublishAsync(channel, message, flags));
		}

		/// <inheritdoc />
		public void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags = CommandFlags.None)
		{
			ExecuteAction(() => _subscriber.Value.Subscribe(channel, handler, flags));
		}

		/// <inheritdoc />
		public ChannelMessageQueue Subscribe(RedisChannel channel, CommandFlags flags = CommandFlags.None)
		{
			return ExecuteAction(() => _subscriber.Value.Subscribe(channel, flags));
		}

		/// <inheritdoc />
		public Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags = CommandFlags.None)
		{
			return ExecuteActionAsync(() => _subscriber.Value.SubscribeAsync(channel, handler, flags));
		}

		/// <inheritdoc />
		public Task<ChannelMessageQueue> SubscribeAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None)
		{
			return ExecuteActionAsync(() => _subscriber.Value.SubscribeAsync(channel, flags));
		}

		/// <inheritdoc />
		public EndPoint SubscribedEndpoint(RedisChannel channel)
		{
			return ExecuteAction(() => _subscriber.Value.SubscribedEndpoint(channel));
		}

		/// <inheritdoc />
		public bool TryWait(Task task)
		{
			return ExecuteAction(() => _subscriber.Value.TryWait(task));
		}

		/// <inheritdoc />
		public void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null, CommandFlags flags = CommandFlags.None)
		{
			ExecuteAction(() => _subscriber.Value.Unsubscribe(channel, handler, flags));
		}

		/// <inheritdoc />
		public void UnsubscribeAll(CommandFlags flags = CommandFlags.None)
		{
			ExecuteAction(() => _subscriber.Value.UnsubscribeAll(flags));
		}

		/// <inheritdoc />
		public Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
		{
			return ExecuteActionAsync(() => _subscriber.Value.UnsubscribeAllAsync(flags));
		}

		/// <inheritdoc />
		public Task UnsubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null, CommandFlags flags = CommandFlags.None)
		{
			return ExecuteActionAsync(() => _subscriber.Value.UnsubscribeAsync(channel, handler, flags));
		}

		/// <inheritdoc />
		public void Wait(Task task)
		{
			ExecuteAction(() => _subscriber.Value.Wait(task));
		}

		/// <inheritdoc />
		public T Wait<T>(Task<T> task)
		{
			return ExecuteAction(() => _subscriber.Value.Wait(task));
		}

		/// <inheritdoc />
		public void WaitAll(params Task[] tasks)
		{
			ExecuteAction(() => _subscriber.Value.WaitAll(tasks));
		}

		#endregion

		private void ResetSubscriber()
		{
			_subscriber = new AtomicLazy<ISubscriber>(_subscriberProvider);
		}

		private void CheckAndReset()
		{
			ResilientConnectionMultiplexer.CheckAndReset(
				_resilientConnectionMultiplexer.LastReconnectTicks,
				ref _lastReconnectTicks,
				_resetLock,
				ResetSubscriber);
		}

		private T ExecuteAction<T>(Func<T> action)
		{
			CheckAndReset();
			return ResilientConnectionMultiplexer.ExecuteAction(_resilientConnectionMultiplexer, action);
		}

		private Task<T> ExecuteActionAsync<T>(Func<Task<T>> action)
		{
			CheckAndReset();
			return ResilientConnectionMultiplexer.ExecuteActionAsync(_resilientConnectionMultiplexer, action);
		}

		private Task ExecuteActionAsync(Func<Task> action)
		{
			CheckAndReset();
			return ResilientConnectionMultiplexer.ExecuteActionAsync(_resilientConnectionMultiplexer, action);
		}

		private void ExecuteAction(System.Action action)
		{
			CheckAndReset();
			ResilientConnectionMultiplexer.ExecuteAction(_resilientConnectionMultiplexer, action);
		}
	}
}
