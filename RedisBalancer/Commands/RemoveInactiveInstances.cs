using Microsoft.Extensions.Logging;
using RedisBalancer.Repositories;
using RedisBalancer.Types;

namespace RedisBalancer.Commands
{
	class RemoveInactiveInstances<TItem>
	{
		private readonly IInstancesRepository<TItem> _repository;
		private readonly RedisBalancerOptions _options;
		private readonly ILogger? _logger;

		public RemoveInactiveInstances(IInstancesRepository<TItem> repository, RedisBalancerOptions options, ILogger? logger)
		{
			_repository = repository;
			_options = options;
			_logger = logger;
		}

		public async Task Run()
		{
			var instances = await _repository.GetAll();

			var now = DateTime.UtcNow;

			var inactiveInstances = instances
				.Where(instance => now - instance.LastPing > _options.InstanceInactivityPeriod)
				.ToArray();

			await _repository.RemoveMany(inactiveInstances);
			
			Log(inactiveInstances);
		}

		private void Log(IInstance<TItem>[] inactiveInstances)
		{
			if (inactiveInstances.Any())
			{
				var names = inactiveInstances.Select(x => x.Name).ToArray();
				var log = string.Join(",", names);
				
				_logger?.LogDebug($"Inactive Instances removed: {log}");
			}
		}
	}
}
