using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Consul;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Grafana;
using Blaze.SimTainer.Service.Providers.CloudStack.Services;
using Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Factories;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Services
{
	public class TestGrafanaDashboardServiceFixture
	{
		private readonly IApplication _mesosApplication;
		private readonly string _base64ServiceName;

		public TestGrafanaDashboardServiceFixture()
		{
			MesosFactory mesosFactory = new MesosFactory();
			string base64Decoded = "test";
			byte[] data = Encoding.ASCII.GetBytes(base64Decoded);
			_base64ServiceName = Convert.ToBase64String(data);

			_mesosApplication = mesosFactory.Create("test", "test-task");
		}

		[Fact]
		public void TestInvalidEndpoint()
		{
			// Arrange
			GrafanaDashboardService grafanaDashboardService =
				new GrafanaDashboardService(string.Empty, string.Empty, new List<string>(), new HttpClient());

			// Act & Assert
			Action act = () => grafanaDashboardService.GetDashboard(_mesosApplication);

			// Assert
			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void TestValidConsulIdResponse()
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
						"[{\"LockIndex\":0,\"Key\":\"test/id\",\"Flags\":0,\"Value\":\"dGVzdA==\",\"CreateIndex\":1,\"ModifyIndex\":1}]")
				});

			// Arrange
			GrafanaDashboardService grafanaDashboardService =
				new GrafanaDashboardService(string.Empty, "http://localhost", new List<string>(),
					new HttpClient(mockMessageHandler.Object));

			// Act
			ConsulKeyValue consulKeyValue = grafanaDashboardService.GetConsulIdKeyValue(_mesosApplication);

			// Assert
			consulKeyValue.Value.Should().Be(_base64ServiceName);
			consulKeyValue.DecodedValue.Should().Be("test");
		}

		[Fact]
		public void TestInvalidConsulResponse()
		{
			// Arrange
			Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
			mockMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.NotFound,
					// Real data from 25-02-2020. Only for the product-identifier-service
					Content = new StringContent(
						"")
				});

			// Arrange
			GrafanaDashboardService grafanaDashboardService =
				new GrafanaDashboardService(string.Empty, "http://localhost", new List<string>(),
					new HttpClient(mockMessageHandler.Object));

			// Act
			ConsulKeyValue consulKeyValue = grafanaDashboardService.GetConsulIdKeyValue(_mesosApplication);

			// Assert
			consulKeyValue.Should().BeNull();
		}

		[Fact]
		public void TestInvalidGrafanaResponse()
		{
			// Arrange
			Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
			mockMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.NotFound,
					// Real data from 25-02-2020. Only for the product-identifier-service
					Content = new StringContent(
						"")
				});

			// Arrange
			GrafanaDashboardService grafanaDashboardService =
				new GrafanaDashboardService("http://localhost", string.Empty, new List<string>(),
					new HttpClient(mockMessageHandler.Object));

			// Act
			GrafanaDashboard grafanaDashboard = grafanaDashboardService.GetGrafanaDashboard("test");

			// Assert
			grafanaDashboard.Should().BeNull();
		}

		[Fact]
		public void TestValidInput()
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
						"[\r\n    {\r\n      \"id\": 1,\r\n      \"uid\": \"123456\",\r\n      \"title\": \"[Team test] test - test-service - metrics\",\r\n      \"uri\": \"db/team-test-test-service-metrics\",\r\n      \"url\": \"/d/team-test-test-service-metrics\",\r\n      \"slug\": \"\",\r\n      \"type\": \"dash-db\",\r\n      \"tags\": [\r\n        \"dev\",\r\n        \"nl.wehkamp\"\r\n      ],\r\n      \"isStarred\": false\r\n    }\r\n]")
				});

			ConsulKeyValue consulKeyValue = new ConsulKeyValue {Value = _base64ServiceName};

			Mock<GrafanaDashboardService> grafanaDashboardServiceMock =
				new Mock<GrafanaDashboardService>("http://localhost/", "http://localhost/", new List<string>(),
					new HttpClient(mockMessageHandler.Object));

			grafanaDashboardServiceMock.Setup(x => x.GetConsulIdKeyValue(It.IsAny<IApplication>()))
				.Returns(consulKeyValue);

			// Act
			string dashboardUrl = grafanaDashboardServiceMock.Object.GetDashboard(_mesosApplication);

			// Assert
			dashboardUrl.Should().NotBeNullOrEmpty();
			dashboardUrl.Should().Be("http://localhost/d/team-test-test-service-metrics");
		}


		[Fact]
		public void TestEmptyResultByRequiredTag()
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
						"[\r\n    {\r\n      \"id\": 1,\r\n      \"uid\": \"123456\",\r\n      \"title\": \"[Team test] test - test-service - metrics\",\r\n      \"uri\": \"db/team-test-test-service-metrics\",\r\n      \"url\": \"/d/team-test-test-service-metrics\",\r\n      \"slug\": \"\",\r\n      \"type\": \"dash-db\",\r\n      \"tags\": [\r\n        \"dev\",\r\n        \"nl.wehkamp\"\r\n      ],\r\n      \"isStarred\": false\r\n    }\r\n]")
				});

			ConsulKeyValue consulKeyValue = new ConsulKeyValue {Value = _base64ServiceName};

			Mock<GrafanaDashboardService> grafanaDashboardServiceMock =
				new Mock<GrafanaDashboardService>("http://localhost/", "http://localhost/",
					new List<string> {"required-tag"}, new HttpClient(mockMessageHandler.Object));

			grafanaDashboardServiceMock.Setup(x => x.GetConsulIdKeyValue(It.IsAny<IApplication>()))
				.Returns(consulKeyValue);

			// Act
			string dashboardUrl = grafanaDashboardServiceMock.Object.GetDashboard(_mesosApplication);

			// Assert
			dashboardUrl.Should().BeNullOrEmpty();
		}

		[Fact]
		public void TestResultsByRequiredTag()
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
						"[\r\n    {\r\n      \"id\": 1,\r\n      \"uid\": \"123456\",\r\n      \"title\": \"[Team test] test - test-service - metrics\",\r\n      \"uri\": \"db/team-test-test-service-metrics\",\r\n      \"url\": \"/d/team-test-test-service-metrics\",\r\n      \"slug\": \"\",\r\n      \"type\": \"dash-db\",\r\n      \"tags\": [\r\n        \"dev\",\r\n        \"nl.wehkamp\"\r\n      ],\r\n      \"isStarred\": false\r\n    }\r\n]")
				});

			ConsulKeyValue consulKeyValue = new ConsulKeyValue {Value = _base64ServiceName};

			Mock<GrafanaDashboardService> grafanaDashboardServiceMock =
				new Mock<GrafanaDashboardService>("http://localhost/", "http://localhost/",
					new List<string> {"nl.wehkamp", "dev"}, new HttpClient(mockMessageHandler.Object));

			grafanaDashboardServiceMock.Setup(x => x.GetConsulIdKeyValue(It.IsAny<IApplication>()))
				.Returns(consulKeyValue);

			// Act
			string dashboardUrl = grafanaDashboardServiceMock.Object.GetDashboard(_mesosApplication);

			// Assert
			dashboardUrl.Should().NotBeNullOrEmpty();
			dashboardUrl.Should().Be("http://localhost/d/team-test-test-service-metrics");
		}

	}
}