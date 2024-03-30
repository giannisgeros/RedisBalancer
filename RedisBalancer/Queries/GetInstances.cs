using RedisBalancer.Repositories;
using RedisBalancer.Types;

namespace RedisBalancer.Queries
{
	public interface IGetInstances<TItem>
	{
		Task<IInstance<TItem>[]> GetAll();
		Task<IInstance<TItem>?> TryGet(string instanceName);
	}

	class GetInstances<TItem> : IGetInstances<TItem>
	{
		private readonly IInstancesRepository<TItem> _repository;

		public GetInstances(IInstancesRepository<TItem> repository)
		{
			_repository = repository;
		}

		public async Task<IInstance<TItem>[]> GetAll()
		{
			var instances = await _repository.GetAll();

			return instances;
		}

		public async Task<IInstance<TItem>?> TryGet(string instanceName)
		{
			var instance = await _repository.TryGet(instanceName);

			return instance;
		}
	}
}
