using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedisBalancer.Repositories;
using RedisBalancer.Types;
using RedisBalancer.Utils;

namespace RedisBalancer.Commands
{
	class DistributeItems<TItem>
		where TItem : IEquatable<TItem>
	{
		private readonly IInstancesRepository<TItem> _repository;
		private readonly IITemsProvider<TItem> _itemsProvider;
		private readonly IAddItemsUtils<TItem> _addItemsUtils;
		private readonly IRemoveItemsUtils<TItem> _removeItemsUtils;
		private readonly ILogger? _logger;

		public DistributeItems(IInstancesRepository<TItem> repository, IITemsProvider<TItem> itemsProvider, IAddItemsUtils<TItem> addItemsUtils, IRemoveItemsUtils<TItem> removeItemsUtils, ILogger? logger)
		{
			_repository = repository;
			_itemsProvider = itemsProvider;
			_addItemsUtils = addItemsUtils;
			_removeItemsUtils = removeItemsUtils;
			_logger = logger;
		}

		public async Task Run()
		{
			var instances = await _repository.GetAll();

			if (!instances.Any())
				throw new Exception($"No active entities exist in order to distribute items");

			var items = await _itemsProvider.GetAll();

			_logger?.LogDebug($"Instances before distribution. Instances: {JsonConvert.SerializeObject(instances)}");

			_removeItemsUtils.RemoveItems(items, instances);

			_logger?.LogDebug($"Instances after items removal. Instances: {JsonConvert.SerializeObject(instances)}");

			_addItemsUtils.AddItems(items, instances);

			_logger?.LogDebug($"Instances after items addition. Instances: {JsonConvert.SerializeObject(instances)}");

			await _repository.UpdateMany(instances);
		}
	}
}
