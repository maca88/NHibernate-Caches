using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate.Cache;
using NHibernate.Cache.Entry;
using NHibernate.Intercept;
using NHibernate.Properties;
using NHibernate.Type;
using NHibernate.UserTypes;
using StackExchange.Redis;

namespace NHibernate.Caches.StackExRedis.Tests
{
	/// <summary>
	/// A redis serializer that uses Json.Net to serialize the data into json. Each <see cref="IUserType"/>
	/// has to be registered explicitly by <see cref="RegisterType"/> method.
	/// </summary>
	public partial class JsonRedisSerializer : IRedisSerializer
	{
		// When deserializing an array of objects all numbers will be deserialized as double or long by default.
		// In order to prevent that we have to add the type metadata so that Json.Net will correctly deserialize
		// the number to the original type.
		private static readonly Dictionary<System.Type, string> ExplicitTypes = new Dictionary<System.Type, string>
		{
			{typeof(short), "s"},
			{typeof(int), "i"},
			{typeof(sbyte), "sb"},
			{typeof(byte), "b"},
			{typeof(decimal), "d"},
			{typeof(float), "f"},
			{typeof(Guid), "g"},
			{typeof(char), "c"},
			{typeof(TimeSpan), "ts"},
			{typeof(DateTimeOffset), "do"}
		};

		// The types that are allowed by default to be serialized and deserialized in order to prevent
		// exposing any security vulnerability as we are not using TypeNameHandling.None
		private static readonly Dictionary<System.Type, string> TypeAliases =
			new Dictionary<System.Type, string>(ExplicitTypes)
			{
				{ typeof(long), "l"},
				{ typeof(double), "db"},
				// Used by NHibernate
				{typeof(object[]), "oa"},
				{typeof(byte[]), "ba"},
				{typeof(List<object>), "lo"},
				{typeof(Hashtable), "ht"},
				{typeof(CacheEntry), "ce"},
				{typeof(CacheLock), "cl"},
				{typeof(CachedItem), "ci"},
				{typeof(CollectionCacheEntry), "cc"},
				{typeof(AnyType.ObjectTypeCacheEntry), "at"},
				{typeof(UnfetchedLazyProperty), "ul"},
				{typeof(UnknownBackrefProperty), "ub"}
			};

		private readonly JsonSerializer _serializer;
		private readonly ExplicitSerializationBinder _serializationBinder = new ExplicitSerializationBinder();

		public JsonRedisSerializer()
		{
			var settings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto,
				DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
				Formatting = Formatting.None,
				SerializationBinder = _serializationBinder
			};
			settings.Converters.Add(new ExplicitTypesConverter());
			_serializer = JsonSerializer.CreateDefault(settings);
		}

		/// <summary>
		/// Register a type that is allowed to be serialized with an alias.
		/// </summary>
		/// <param name="type">The type allowed to be serialized.</param>
		/// <param name="alias">The shorten name of the type.</param>
		public void RegisterType(System.Type type, string alias)
		{
			_serializationBinder.RegisterType(type, alias);
		}

		/// <inheritdoc />
		public object Deserialize(RedisValue value)
		{
			using (var reader = new CustomJsonTextReader(new StringReader(value)))
			{
				return _serializer.Deserialize(reader);
			}
		}

		/// <inheritdoc />
		public RedisValue Serialize(object value)
		{
			using (var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture))
			using (var writer = new CustomJsonTextWriter(stringWriter))
			{
				writer.Formatting = _serializer.Formatting;
				_serializer.Serialize(writer, value, typeof(object));
				return stringWriter.ToString();
			}
		}

		private class ExplicitSerializationBinder : DefaultSerializationBinder
		{
			private readonly Dictionary<System.Type, string> _typeAliases;
			private readonly Dictionary<string, System.Type> _aliasTypes;

			public ExplicitSerializationBinder()
			{
				_typeAliases = new Dictionary<System.Type, string>(TypeAliases);
				_aliasTypes = _typeAliases.ToDictionary(o => o.Value, o => o.Key);
			}

			public void RegisterType(System.Type type, string alias)
			{
				if (string.IsNullOrEmpty(alias))
				{
					alias = type.AssemblyQualifiedName;
				}
				if (_typeAliases.ContainsKey(type))
				{
					throw new InvalidOperationException($"Type {type} is already registered.");
				}
				if (_aliasTypes.ContainsKey(alias))
				{
					throw new InvalidOperationException($"Alias {alias} is already registered.");
				}
				_typeAliases.Add(type, alias);
				_aliasTypes.Add(alias, type);
			}

			public override void BindToName(System.Type serializedType, out string assemblyName, out string typeName)
			{
				if (!_typeAliases.TryGetValue(serializedType, out typeName))
				{
					throw new InvalidOperationException($"Unknown type '{serializedType.AssemblyQualifiedName}'");
				}

				assemblyName = null;
			}

			public override System.Type BindToType(string assemblyName, string typeName)
			{
				if (!_aliasTypes.TryGetValue(typeName, out var type))
				{
					throw new InvalidOperationException($"Unknown type '{typeName}, {assemblyName}'");
				}

				return type;
			}
		}

		/// <summary>
		/// A json converter that adds the type metadata for <see cref="JsonRedisSerializer.ExplicitTypes"/>.
		/// </summary>
		private class ExplicitTypesConverter : JsonConverter
		{
			public override bool CanConvert(System.Type objectType)
			{
				return ExplicitTypes.ContainsKey(objectType);
			}

			public override bool CanRead => false;

			public override bool CanWrite => true;

			public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
			{
				throw new NotSupportedException();
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("$t");
				var typeName = ExplicitTypes[value.GetType()];
				writer.WriteValue(typeName);
				writer.WritePropertyName("$v");
				writer.WriteValue(value);
				writer.WriteEndObject();
			}
		}

		#region Reader & Writer

		/// <summary>
		/// Renames the shorten the metadata properties names to the original ones.
		/// </summary>
		private partial class CustomJsonTextReader : JsonTextReader
		{
			public CustomJsonTextReader(TextReader reader) : base(reader)
			{
			}

			public override bool Read()
			{
				var hasToken = base.Read();
				if (!hasToken || TokenType != JsonToken.PropertyName || !(Value is string str))
				{
					return hasToken;
				}
				switch (str)
				{
					case "$t":
						SetToken(JsonToken.PropertyName, "$type");
						break;
					case "$v":
						SetToken(JsonToken.PropertyName, "$value");
						break;
					case "$vs":
						SetToken(JsonToken.PropertyName, "$values");
						break;
				}
				return true;
			}
		}

		/// <summary>
		/// Reduces the json size by shorten the metadata properties names.
		/// </summary>
		private partial class CustomJsonTextWriter : JsonTextWriter
		{
			public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter)
			{
			}

			public override void WritePropertyName(string name, bool escape)
			{
				switch (name)
				{
					case "$type":
						name = "$t";
						break;
					case "$value":
						name = "$v";
						break;
					case "$values":
						name = "$vs";
						break;
				}
				base.WritePropertyName(name, escape);
			}
		}

		#endregion
	}
}
