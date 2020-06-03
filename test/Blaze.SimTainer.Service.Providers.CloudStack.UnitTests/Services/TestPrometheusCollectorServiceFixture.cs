using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Prometheus;
using Blaze.SimTainer.Service.Providers.CloudStack.Services.ApiCollectors;
using Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Factories;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Services
{
	public class TestPrometheusCollectorServiceFixture
	{
		private const string TestServiceJob = "product-identifier";

		private readonly MesosFactory _mesosFactory;
		private readonly PrometheusFactory _prometheusFactory;

		public TestPrometheusCollectorServiceFixture()
		{
			_mesosFactory = new MesosFactory();
			_prometheusFactory = new PrometheusFactory();
		}

		[Fact]
		public void TestGetMetricListWithValidInput()
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
						"{\"status\":\"success\",\"data\":{\"resultType\":\"vector\",\"result\":[{\"metric\":{},\"value\":[1583837746.229,\"109054569.08296943\"]}]}}")
				});
			PrometheusCollector prometheusCollector =
				new PrometheusCollector("http://localhost", new HttpClient(mockMessageHandler.Object));

			// Act
			List<PrometheusResult> result =
				prometheusCollector.GetMetricList("sum(avg(service_process_memory_used_bytes)) by(role,instance)");

			// Assert
			result.Should().NotBeNullOrEmpty();
			result[0].Value[0].Should().Be("1583837746.229");
			result[0].Value[1].Should().Be("109054569.08296943");
		}

		[Fact]
		public void TestGetMetricListWithInvalidStatusCode()
		{
			// Arrange
			Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
			mockMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.BadGateway,
					// Real data from 25-02-2020. Only for the product-identifier-service
					Content = new StringContent("")
				});
			PrometheusCollector mesosCollector =
				new PrometheusCollector("http://localhost", new HttpClient(mockMessageHandler.Object));

			// Act
			List<PrometheusResult> result =
				mesosCollector.GetMetricList("sum(avg(service_process_memory_used_bytes)) by(role,instance)");

			// Assert
			result.Should().BeEmpty();
		}

		[Fact]
		public void TestGetMetricListWithInvalidInput()
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
					Content = new StringContent("OK")
				});
			PrometheusCollector mesosCollector =
				new PrometheusCollector("http://localhost", new HttpClient(mockMessageHandler.Object));

			// Act
			List<PrometheusResult> result =
				mesosCollector.GetMetricList("sum(avg(service_process_memory_used_bytes)) by(role,instance)");

			// Assert
			result.Should().BeEmpty();
		}

		[Fact]
		public void TestGetMetrics()
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
						"{\"status\":\"success\",\"data\":{\"resultType\":\"vector\",\"result\":[{\"metric\":{},\"value\":[1583837746.229,\"109054569.08296943\"]}]}}")
				});
			PrometheusCollector mesosCollector =
				new PrometheusCollector("http://localhost", new HttpClient(mockMessageHandler.Object));
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";
			IApplication mesosApp = _mesosFactory.Create(serviceName, taskIdentifier);
			List<IApplication> applications = new List<IApplication> {mesosApp};

			Mock<PrometheusCollector> prometheusCollectorMock =
				new Mock<PrometheusCollector>(null, new HttpClient(mockMessageHandler.Object));
			PrometheusResult metricResult1 = _prometheusFactory.Create(mesosApp, TestServiceJob);
			List<PrometheusResult> prometheusResult = new List<PrometheusResult> {metricResult1};
			prometheusCollectorMock.Setup(x => x.GetMetricList(It.IsAny<string>()))
				.Returns(prometheusResult);
			// Act

			IMetric result = prometheusCollectorMock.Object.GetMetrics(applications[0].Instances[0]);

			// Assert
			result.Values.Should().ContainKey("cpu_usage");
			result.Values.Should().ContainKey("memory_usage");
		}
	}
}