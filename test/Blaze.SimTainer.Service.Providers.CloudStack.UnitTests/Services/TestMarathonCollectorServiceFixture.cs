using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Marathon;
using Blaze.SimTainer.Service.Providers.CloudStack.Services.ApiCollectors;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Services
{
	public class TestMarathonCollectorServiceFixture
	{
		[Fact]
		public void TestMarathonCollectorWrongEndpoint()
		{
			// Arrange
			MarathonCollector marathonCollector = new MarathonCollector(string.Empty, new HttpClient());

			// Act & Assert
			Action act = () => marathonCollector.GetAllApplications();

			// Assert
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void TestMarathonCollectorValidApplicationsListByJsonInput()
		{
			// Arrange
			Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
			mockMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					// Real data from 25-02-2020. Only for the product-identifier-service
					Content = new StringContent(
						"{\r\n    \"apps\": [\r\n        {\r\n            \"id\": \"/blaze-product-identifier-service\",\r\n            \"versionInfo\": {\r\n                \"lastScalingAt\": \"2019-08-30T11:16:29.601Z\",\r\n                \"lastConfigChangeAt\": \"2019-08-30T11:16:29.601Z\"\r\n            }\r\n        }\r\n    ]\r\n}")
				});

			MarathonCollector marathonCollector =
				new MarathonCollector("http://localhost", new HttpClient(mockMessageHandler.Object));

			// Act
			IList<MarathonApp> marathonApps = marathonCollector.GetAllApplications();

			// Assert
			marathonApps.Should().NotBeNullOrEmpty();
			marathonApps.Should().Contain(x => x.Identifier == "/blaze-product-identifier-service");
		}

		[Fact]
		public void TestMarathonCollectorInvalidApplicationsListByJsonInput()
		{
			// Arrange
			Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
			mockMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("InvalidJson")
				});

			MarathonCollector marathonCollector =
				new MarathonCollector("http://localhost", new HttpClient(mockMessageHandler.Object));

			// Act
			IList<MarathonApp> marathonApps = marathonCollector.GetAllApplications();

			// Assert
			marathonApps.Should().BeEmpty();
		}

		[Fact]
		public void TestMarathonCollector500Error()
		{
			// Arrange
			Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
			mockMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Content = new StringContent("Something went wrong")
				});

			MarathonCollector marathonCollector =
				new MarathonCollector("http://localhost", new HttpClient(mockMessageHandler.Object));

			// Act
			IList<MarathonApp> marathonApps = marathonCollector.GetAllApplications();

			// Assert
			marathonApps.Should().BeEmpty();
		}
	}
}