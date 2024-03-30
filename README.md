# RedisBalancer

RedisBalancer is a library that utilizes redis in order to distribute items amongst multiple instances equally.

## Description

### Types
This library has one basic type **Instance<TItem>**. **Instance** being the current instance that is running the library and **TItem** being any class that implements **IEquitable<TItem>**.
The idea is that every **Instance** holds some **Items** and each time that the distribution job runs, it can correctly find and remove or add - distribute - the **Items** across the **Instances**.  

### Jobs
The library implements the basic c# PeriodicTimer and has 3 jobs.
1. **Ping**: Informs that this **Instance** is active
2. **RemoveInactiveInstances**: Checks for any **Instance** that has outdated Ping time and removes it.
3. **DistributeItems**: Gets the items to be distributed via an **ItemsProvider service** that should be provided to the library and distributes them amongst the **active Instances**

### Exposed Services
This library also expose a **IGetInstances<TItem> service** that can be used to get all **Instances** or get an **Instance** by name.

### Options
When a project wants to use the library, it will require some options (**RedisBalancerOptions**). The properties are:
1. **RedisKey** [required]: A *string* key that is used as a **RedisKey**. The **RedisValue** is an array of **Instances**. So all the **Instances** that use the same **RedisKey** will be used in order to distribute the **Items**.
2. **InstanceName** [required]: A *string* which is used for the name of the **Instance**.
3. **RedisConnectionString** [required]: A *string* which is used for the connection string of the redis to be connected to.
4. **PingInterval** [required]: A *TimeSpan* which is used for the **Ping** job interval.
5. **RemoveInactiveInstancesInterval**: A *TimeSpan* which is used for the **RemoveInactiveInstances** job interval. If not set then **RemoveInactiveInstancesInterval** = **PingInterval** * 2.
6. **DistributeItemsInterval**: A *TimeSpan* which is used for the **DistributeItems** job interval. If not set then **DistributeItemsInterval** = **PingInterval** * 2.
7. **InstanceInactivityPeriod**: A *TimeSpan* used by the **RemoveInactiveInstances** job, in order to determine the outdated **Instances**. If not set then **InstanceInactivityPeriod** = **PingInterval** * 3.
8. **RedisKeyExpiration**: A *TimeSpan* that is used for the **RedisKey** expiration. If not set, then **RedisKey** will have no expiration and should be removed manually if needed.

## Logic
There is a **Main** class that starts when the application starts as a Hosted Service, that is responsible for the jobs. Each time it runs (using the PeriodicTimer), it gets a *redlock* based on the **RedisKey** provided (in order to avoid multiple **Instances** to run the jobs simultaneously) and runs the jobs.

## Usage

Using net core DI.
```c#
var options = new RedisBalancerOptions(
    redisKey: "ItemsBalanceKey",
	instanceName: "Instance_1",
	redisConnectionString: "127.0.0.1:6379",
	pingInterval: TimeSpan.FromSeconds(5),
	redisKeyExpiration: TimeSpan.FromSeconds(50));

services.AddRedisBalancer(
	options,
	serviceProvider => serviceProvider.GetRequiredService<IITemsProvider<Item>>(),
	serviceProvider =>
	{
		var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
		
        return loggerFactory.CreateLogger("Instance_1-RedisBalancer");
	}
```

Included in the solution is an example project that uses the library.

## Installation

[RedisBalancer nuget.org link](https://www.nuget.org/packages/RedisBalancer/)

Use the Nuget Package Manager from Visual Studio or install via cli

```bash
dotnet add package RedisBalancer
```

## License

[MIT](https://choosealicense.com/licenses/mit/)
