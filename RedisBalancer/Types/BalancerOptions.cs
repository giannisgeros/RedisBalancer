namespace RedisBalancer.Types
{
	public class RedisBalancerOptions
	{
		public string InstanceName { get; }
		public string RedisKey { get; }
		public string RedisConnectionString { get; }
		public TimeSpan PingInterval { get; }
		public TimeSpan RemoveInactiveInstancesInterval { get; }
		public TimeSpan DistributeItemsInterval { get; }
		public TimeSpan InstanceInactivityPeriod { get; }
		public TimeSpan? RedisKeyExpiration { get; }

		public RedisBalancerOptions(string instanceName, string redisKey, string redisConnectionString, TimeSpan pingInterval, TimeSpan? removeInactiveInstancesInterval = null, TimeSpan? distributeItemsInterval = null, TimeSpan? instanceInactivityPeriod = null, TimeSpan? redisKeyExpiration = null)
		{
			InstanceName = instanceName;
			RedisKey = redisKey;
			RedisConnectionString = redisConnectionString;
			PingInterval = pingInterval;
			RemoveInactiveInstancesInterval = removeInactiveInstancesInterval ?? pingInterval * 2;
			DistributeItemsInterval = distributeItemsInterval ?? pingInterval * 2;
			InstanceInactivityPeriod = instanceInactivityPeriod ?? pingInterval * 3;
			RedisKeyExpiration = redisKeyExpiration;
		}
	}
}
