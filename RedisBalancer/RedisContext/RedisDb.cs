using Newtonsoft.Json;
using RedisBalancer.Types;
using StackExchange.Redis;

namespace RedisBalancer.RedisContext
{
	interface IRedisDb
	{
		Task<TValue?> TryGet<TValue>(string key)
			where TValue : class;
		void Set<TValue>(string key, TValue value, TimeSpan? expiration = null)
			where TValue : class;
	}

	class RedisDb : IRedisDb, IAsyncDisposable
	{
		private readonly IDatabase _db;
		private readonly Lazy<ITransaction> _transaction;
		private readonly JsonSerializerSettings _serializerSettings;

		public RedisDb(IDatabase db)
		{
			_db = db;
			_transaction = new Lazy<ITransaction>(() => db.CreateTransaction());
			_serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
		}

		public async Task<TValue?> TryGet<TValue>(string key)
			where TValue : class
		{
			var entry = await _db.StringGetAsync(key);

			if (!entry.HasValue)
				return null;

			var entity = DeSerialize<TValue>(entry!);

			return entity;
		}

		public void Set<TValue>(string key, TValue value, TimeSpan? expiration = null)
			where TValue : class
		{
			var entry = Serialize(value);

			_ = _transaction.Value.StringSetAsync(key, entry);

			if (expiration is not null)
				_ = _transaction.Value.KeyExpireAsync(key, expiration);
		}

		public async Task SaveChangesAsync()
		{
			if (!_transaction.IsValueCreated)
				return;

			var committed = await _transaction.Value.ExecuteAsync();

			if (!committed)
				throw new RedisDbTransactionException();
		}

		private string Serialize<T>(T obj)
			=> JsonConvert.SerializeObject(obj, _serializerSettings);
		private T DeSerialize<T>(string value)
			=> JsonConvert.DeserializeObject<T>(value, _serializerSettings) ?? throw new Exception($"Could not deserialize {value} to {typeof(T).FullName}");

		public ValueTask DisposeAsync()
		{
			return ValueTask.CompletedTask;
		}
	}
}
