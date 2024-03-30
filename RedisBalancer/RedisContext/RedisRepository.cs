using RedisBalancer.Types;
using StackExchange.Redis;

namespace RedisBalancer.RedisContext
{
	interface IRedisRepository<TEntity>
	{
		Task<TEntity[]> GetAll(string redisKey);
		Task<TEntity> Get(string redisKey, Func<TEntity, bool> selector);
		Task<TEntity?> TryGet(string redisKey, Func<TEntity, bool> selector);
		Task Add(string redisKey, TEntity entity);
		Task Update(string redisKey, Func<TEntity, bool> selector, TEntity entity);
		Task UpdateMany(string redisKey, Func<TEntity, bool> selector, TEntity[] entities);
		Task Remove(string redisKey, Func<TEntity, bool> selector);
		Task RemoveMany(string redisKey, Func<TEntity, bool> selector, TEntity[] entitites);
	}

	class RedisRepository<TEntity> : IRedisRepository<TEntity>
	{
		private readonly IConnectionMultiplexer _connectionMultiplexer;
		private readonly RedisBalancerOptions _options;

		public RedisRepository(IConnectionMultiplexer connectionMultiplexer, RedisBalancerOptions options)
		{
			_connectionMultiplexer = connectionMultiplexer;
			_options = options;
		}

		public async Task<TEntity[]> GetAll(string redisKey)
		{
			await using var db = new RedisDb(_connectionMultiplexer.GetDatabase());

			var entries = await db.TryGet<TEntity[]>(redisKey);

			return entries ?? Array.Empty<TEntity>();
		}

		public async Task<TEntity> Get(string redisKey, Func<TEntity, bool> selector)
		{
			return await TryGet(redisKey, selector) ?? throw new Exception($"Could not find entity");
		}

		public async Task<TEntity?> TryGet(string redisKey, Func<TEntity, bool> selector)
		{
			var entries = await GetAll(redisKey);

			return entries.FirstOrDefault(selector);
		}

		public async Task Add(string redisKey, TEntity entity)
		{
			await using var db = new RedisDb(_connectionMultiplexer.GetDatabase());

			var entries = await db.TryGet<List<TEntity>>(redisKey);

			if (entries is not null)
				entries.Add(entity);
			else
				entries = new List<TEntity> { entity };

			db.Set(redisKey, entries, _options.RedisKeyExpiration);

			await db.SaveChangesAsync();
		}

		public async Task Update(string redisKey, Func<TEntity, bool> selector, TEntity entity)
		{
			await using var db = new RedisDb(_connectionMultiplexer.GetDatabase());

			var entries = await db.TryGet<List<TEntity>>(redisKey) ?? throw new Exception("Update failed. Redis entry is null");

			var existingEntry = entries.FirstOrDefault(selector) ?? throw new Exception("Update failed. Could not find entity");

			entries.Remove(existingEntry);
			entries.Add(entity);

			db.Set(redisKey, entries, _options.RedisKeyExpiration);

			await db.SaveChangesAsync();
		}

		public async Task UpdateMany(string redisKey, Func<TEntity, bool> selector, TEntity[] entities)
		{
			if (!entities.Any())
				return;

			await using var db = new RedisDb(_connectionMultiplexer.GetDatabase());

			var entries = await db.TryGet<List<TEntity>>(redisKey) ?? throw new Exception("UpdateMany failed. Redis entry is null");

			var existingEntries = entries.Where(selector).ToArray();

			foreach (var entry in existingEntries)
				entries.Remove(entry);

			entries.AddRange(entities);

			db.Set(redisKey, entries, _options.RedisKeyExpiration);

			await db.SaveChangesAsync();
		}

		public async Task Remove(string redisKey, Func<TEntity, bool> selector)
		{
			await using var db = new RedisDb(_connectionMultiplexer.GetDatabase());

			var entries = await db.TryGet<List<TEntity>>(redisKey) ?? throw new Exception("Remove failed. Redis entry is null");

			var entry = entries.FirstOrDefault(selector) ?? throw new Exception("Remove failed. Could not find entity");

			entries.Remove(entry);

			db.Set(redisKey, entries, _options.RedisKeyExpiration);

			await db.SaveChangesAsync();
		}

		public async Task RemoveMany(string redisKey, Func<TEntity, bool> selector, TEntity[] entitites)
		{
			if (!entitites.Any())
				return;

			await using var db = new RedisDb(_connectionMultiplexer.GetDatabase());

			var entries = await db.TryGet<List<TEntity>>(redisKey) ?? throw new Exception("RemoveMany failed. Redis entry is null");

			var existingEntries = entries.Where(selector).ToArray();

			foreach (var entry in existingEntries)
				entries.Remove(entry);

			db.Set(redisKey, entries, _options.RedisKeyExpiration);

			await db.SaveChangesAsync();
		}
	}
}
