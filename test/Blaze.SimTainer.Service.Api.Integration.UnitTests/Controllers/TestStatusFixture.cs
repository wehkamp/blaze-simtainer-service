using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Blaze.SimTainer.Service.Api.Integration.UnitTests.Controllers
{
	public class TestStatusFixture : IDisposable
	{
		private readonly TestServer _testServer;

		public TestStatusFixture()
		{
			IWebHostBuilder builder = new WebHostBuilder()
				.UseEnvironment("Production")
				.ConfigureServices(ConfigureMockedServices)
				.UseStartup<Startup>();

			_testServer = new TestServer(builder);

			Client = _testServer.CreateClient();
		}

		public HttpClient Client { get; }


		[Fact]
		public void TestStatusEndpointController()
		{
			// Arrange & Act
			Task<string> result = Client.GetStringAsync("/status");
			JObject obj = JObject.Parse(result.Result);
			// Assert
			obj["status"].ToString().Should().Be("OK");
		}

		public void Dispose()
		{
			Client.Dispose();
			_testServer.Dispose();
		}

		private void ConfigureMockedServices(IServiceCollection services)
		{
		}
	}
}