using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Blaze.SimTainer.Service.Api.Integration.UnitTests.Controllers
{
	public class TestCloudStackControllerFixture : IDisposable
	{
		private readonly TestServer _testServer;

		public TestCloudStackControllerFixture()
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
		public void TestGetGameControllerFor200()
		{
			// Arrange & Act
			Task<HttpResponseMessage> result = Client.GetAsync("/v1/cloudstack/game");
			HttpResponseMessage requestResult = result.Result;

			// Assert
			requestResult.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public void TestDeleteGameControllerExists()
		{
			// Arrange & Act
			Task<HttpResponseMessage> result = Client.DeleteAsync("/v1/cloudstack/game?identifier=123&force=false");
			HttpResponseMessage requestResult = result.Result;

			// Assert
			requestResult.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
			requestResult.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
			requestResult.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
		}


		[Fact]
		public void TestDeleteGameControllerExistsWithoutQueryParams()
		{
			// Arrange & Act
			Task<HttpResponseMessage> result = Client.DeleteAsync("/v1/cloudstack/game");
			HttpResponseMessage requestResult = result.Result;

			// Assert
			requestResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		}


		[Fact]
		public void TestHubControllerExists()
		{
			// Arrange & Act
			Task<HttpResponseMessage> result = Client.GetAsync("/hubs/cloudstack/game");
			HttpResponseMessage requestResult = result.Result;

			// Assert
			requestResult.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
			requestResult.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
		}

		[Fact]
		public void TestGetDashboardUrlWithQueryParams()
		{
			// Arrange & Act
			Task<HttpResponseMessage> result = Client.GetAsync("/v1/cloudstack/game/url?identifier=test-test");
			HttpResponseMessage requestResult = result.Result;

			// Assert
			requestResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Fact]
		public void TestGetDashboardUrlWithoutQueryParams()
		{
			// Arrange & Act
			Task<HttpResponseMessage> result = Client.GetAsync("/v1/cloudstack/game/url");
			HttpResponseMessage requestResult = result.Result;

			// Assert
			requestResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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