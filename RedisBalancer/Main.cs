using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisBalancer.Commands;
using RedisBalancer.Types;
using RedLockNet;

[assembly: InternalsVisibleTo("RedisBalancerTests")]
namespace RedisBalancer
{
	class Main<TItem> : IHostedService
		where TItem : IEquatable<TItem>
	{
		private readonly string _redisKey;
		private readonly PeriodicTimer _timer;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly TimeSpan _pingInterval;
		private readonly TimeSpan _removeInactiveInstancesInterval;
		private readonly TimeSpan _distributeItemsInterval;
		private DateTime _pingLastRun = DateTime.MinValue;
		private DateTime _distributeItemsLastRun = DateTime.MinValue;
		private DateTime _removeInactiveInstancesLastRun = DateTime.MinValue;
		private readonly Ping<TItem> _ping;
		private readonly RemoveInactiveInstances<TItem> _removeInactiveInstances;
		private readonly DistributeItems<TItem> _distributeItems;
		private readonly IDistributedLockFactory _lockFactory;
		private readonly ILogger? _logger;

		public Main(Ping<TItem> ping, RemoveInactiveInstances<TItem> removeInactiveInstances, DistributeItems<TItem> distributeItems, IDistributedLockFactory lockFactory, RedisBalancerOptions options, ILogger? logger)
		{
			_ping = ping;
			_removeInactiveInstances = removeInactiveInstances;
			_distributeItems = distributeItems;
			_lockFactory = lockFactory;
			_logger = logger;
			_redisKey = options.RedisKey;
			_pingInterval = options.PingInterval;
			_distributeItemsInterval = options.DistributeItemsInterval;
			_removeInactiveInstancesInterval = options.RemoveInactiveInstancesInterval;

			_timer = new PeriodicTimer(_pingInterval);
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public Task StartAsync(CancellationToken _)
		{
			Task.Run(async () => await Run(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

			_logger?.LogDebug("Timer started");

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken _)
		{
			_cancellationTokenSource.Cancel();

			_cancellationTokenSource.Dispose();

			_timer.Dispose();

			_logger?.LogDebug("Timer disposed");

			return Task.CompletedTask;
		}

		private async Task Run(CancellationToken cancellationToken)
		{
			try
			{
				while (await _timer.WaitForNextTickAsync(cancellationToken))
				{
					try
					{
						var now = DateTime.UtcNow;

						using var redLock = _lockFactory.CreateLock($"{_redisKey}-lock", _pingInterval, _pingInterval * 2, TimeSpan.FromMilliseconds(100));
						if (!redLock.IsAcquired)
						{
							_logger?.LogDebug("Redlock was not acquired");

							return;
						}

						await Ping(now);

						await RemoveInactiveInstances(now);

						await DistributeItems(now);
					}
					catch (Exception ex)
					{
						_logger?.LogError(ex, $"Error while executing main logic");
					}
				}
			}
			catch (OperationCanceledException)
			{
				_logger?.LogDebug("Timer stopped");
			}
		}

		private async Task Ping(DateTime now)
		{
			if (now - _pingLastRun < _pingInterval)
				return;

			_logger?.LogDebug("Ping started");

			await _ping.Run();

			_logger?.LogDebug("Ping finished");

			_pingLastRun = now;
		}

		private async Task RemoveInactiveInstances(DateTime now)
		{
			if (now - _removeInactiveInstancesLastRun < _removeInactiveInstancesInterval)
				return;

			_logger?.LogDebug("RemoveInactiveInstances started");

			await _removeInactiveInstances.Run();
			
			_logger?.LogDebug("RemoveInactiveInstances finished");

			_removeInactiveInstancesLastRun = now;
		}

		private async Task DistributeItems(DateTime now)
		{
			if (now - _distributeItemsLastRun < _distributeItemsInterval)
				return;

			_logger?.LogDebug("DistributeItems started");

			await _distributeItems.Run();

			_logger?.LogDebug($"DistributeItems finished");

			_distributeItemsLastRun = now;
		}
	}
}
