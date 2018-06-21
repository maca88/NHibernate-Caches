﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using NHibernate.Cache;
using StackExchange.Redis;
using static NHibernate.Caches.StackExRedis.ConfigurationHelper;

namespace NHibernate.Caches.StackExRedis
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class DefaultRegionStrategy : AbstractRegionStrategy
	{

		/// <inheritdoc />
		public override async Task<object> GetAsync(object key, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await (base.GetAsync(key, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				return await (base.GetAsync(key, cancellationToken)).ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public override async Task<object[]> GetManyAsync(object[] keys, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await (base.GetManyAsync(keys, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				return await (base.GetManyAsync(keys, cancellationToken)).ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public override async Task<string> LockAsync(object key, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await (base.LockAsync(key, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				return await (base.LockAsync(key, cancellationToken)).ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public override async Task<string> LockManyAsync(object[] keys, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await (base.LockManyAsync(keys, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				return await (base.LockManyAsync(keys, cancellationToken)).ConfigureAwait(false);
			}
		}

		/// <inheritdoc />
		public override async Task PutAsync(object key, object value, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				await (base.PutAsync(key, value, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				// Here we don't know if the operation was executed after as successful lock, so
				// the easiest solution is to skip the operation
			}
		}

		/// <inheritdoc />
		public override async Task PutManyAsync(object[] keys, object[] values, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				await (base.PutManyAsync(keys, values, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				// Here we don't know if the operation was executed after as successful lock, so
				// the easiest solution is to skip the operation
			}
		}

		/// <inheritdoc />
		public override async Task<bool> RemoveAsync(object key, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await (base.RemoveAsync(key, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				// There is no point removing the key in the new version.
				return false;
			}
		}

		/// <inheritdoc />
		public override async Task<long> RemoveManyAsync(object[] keys, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await (base.RemoveManyAsync(keys, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				// There is no point removing the keys in the new version.
				return 0L;
			}
		}

		/// <inheritdoc />
		public override async Task<bool> UnlockAsync(object key, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await (base.UnlockAsync(key, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				// If the lock was acquired in the old version we are unable to unlock the key.
				return false;
			}
		}

		/// <inheritdoc />
		public override async Task<int> UnlockManyAsync(object[] keys, string lockValue, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await (base.UnlockManyAsync(keys, lockValue, cancellationToken)).ConfigureAwait(false);
			}
			catch (RedisServerException e) when (e.Message == InvalidVersionMessage)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (InitializeVersionAsync()).ConfigureAwait(false);
				// If the lock was acquired in the old version we are unable to unlock the keys.
				return 0;
			}
		}

		/// <inheritdoc />
		public override async Task ClearAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var results = (RedisValue[]) await (Database.ScriptEvaluateAsync(UpdateVersionLuaScript,
				_regionKeyArray, _maxVersionNumber)).ConfigureAwait(false);
			var version = results[0];
			UpdateVersion(version);
			if (_usePubSub)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await (ConnectionMultiplexer.GetSubscriber().PublishAsync(RegionKey, version)).ConfigureAwait(false);
			}
		}

		private async Task InitializeVersionAsync()
		{
			var results = (RedisValue[]) await (Database.ScriptEvaluateAsync(InitializeVersionLuaScript, _regionKeyArray)).ConfigureAwait(false);
			var version = results[0];
			UpdateVersion(version);
		}
	}
}
