using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Blaze.SimTainer.Service.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(builder =>
				{
					builder.ConfigureKestrel(options => options.AddServerHeader = false)
						.ConfigureAppConfiguration((builderContext, config) =>
						{
							config.AddJsonFile("appsettings.json", false, true);
							config.AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json",
								true);
							config.AddEnvironmentVariables();
						})
						.UseStartup<Startup>();
				});
		}
	}
}