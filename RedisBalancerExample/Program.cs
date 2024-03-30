using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisBalancer;
using RedisBalancer.Queries;
using RedisBalancer.Types;
using StackExchange.Redis;

namespace RedisBalancerExample
{
	public class Program
	{
		private static readonly string _name = $"Instance-{new Random().Next(1, 100)}";

		public static async Task Main(string[] args)
		{
			try
			{
				Console.WriteLine($"{_name}. Started");

				var host = CreateHostBuilder(args).Build();

				Task.Run(async () =>
				{
					Thread.Sleep(15000);

					var getInstances = host.Services.GetRequiredService<IGetInstances<Item>>();
					var instance = await getInstances.TryGet(_name);

					var itemIds = instance?.Items.Select(x => x.Id).ToArray();

					if (itemIds?.Any() == true)
						Console.WriteLine($"{_name}. ItemIds: {string.Join(",", itemIds)}");
				});

				await host.RunAsync();

				Console.WriteLine($"{_name}. Finished");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());

				Console.WriteLine($"{_name}. Finished after error");
			}

			Console.ReadKey();
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(options =>
				{
					options.ClearProviders();
					options.AddConsole();
					options.AddDebug();
					options.SetMinimumLevel(LogLevel.Debug);
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddSingleton<IITemsProvider<Item>>(new ItemService());

					var redisConnectionString = "127.0.0.1:6379";
					services.AddStackExchangeRedisCache(options =>
					{
						options.Configuration = redisConnectionString;
					});

					var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
					services.AddSingleton<IConnectionMultiplexer>(multiplexer);

					var options = new RedisBalancerOptions(
						redisKey: "ItemsBalanceKey",
						instanceName: _name,
						redisConnectionString: redisConnectionString,
						pingInterval: TimeSpan.FromSeconds(5),
						redisKeyExpiration: TimeSpan.FromSeconds(50));

					services.AddRedisBalancer(
						options,
						serviceProvider => serviceProvider.GetRequiredService<IITemsProvider<Item>>(),
						serviceProvider =>
						{
							var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

							return loggerFactory.CreateLogger($"Instance-{_name}-Balancer");
						}
					);
				});
	}
}
