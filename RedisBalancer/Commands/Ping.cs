using Microsoft.Extensions.Logging;
using RedisBalancer.Repositories;
using RedisBalancer.Types;

namespace RedisBalancer.Commands
{
	class Ping<TItem>
		where TItem : IEquatable<TItem>
	{
		private readonly IInstancesRepository<TItem> _repository;
		private readonly RedisBalancerOptions _options;
		private readonly ILogger? _logger;

		public Ping(IInstancesRepository<TItem> repository, RedisBalancerOptions options, ILogger? logger)
		{
			_repository = repository;
			_options = options;
			_logger = logger;
		}

		public async Task Run()
		{
			var instance = await _repository.TryGet(_options.InstanceName);

			if (instance is null)
			{
				var newInstance = new Instance<TItem>(_options.InstanceName, DateTime.UtcNow, new List<TItem>());

				await _repository.Add(newInstance);

				_logger?.LogDebug($"Instance created and pinged");
			}
			else
			{
				instance.Ping();

				await _repository.Update(instance);

				_logger?.LogDebug($"Instance pinged");
			}
		}
	}
}
