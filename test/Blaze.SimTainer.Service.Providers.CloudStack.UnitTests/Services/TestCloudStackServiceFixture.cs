using System;
using System.Collections.Generic;
using System.Linq;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Events;
using Blaze.SimTainer.Service.Providers.CloudStack.Services;
using Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Factories;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Services
{
	public class TestCloudStackServiceFixture
	{
		private readonly MesosFactory _mesosFactory;

		public TestCloudStackServiceFixture()
		{
			_mesosFactory = new MesosFactory();
		}

		[Fact]
		public void TestInvalidBaseUrl()
		{
			// Arrange
			ApiOptions apiOptions = new ApiOptions
				{PrometheusEndpoint = "http://notexisting.url", MesosEndpoint = "http://notexisting.url"};
			Mock<IOptions<ApiOptions>> iOptionsMock = new Mock<IOptions<ApiOptions>>();
			iOptionsMock.Setup(x => x.Value).Returns(apiOptions);
			CloudStackService cloudStackService = new CloudStackService(iOptionsMock.Object);

			// Act
			Action act = () => cloudStackService.Poll();

			// Assert
			act.Should().Throw<InvalidOperationException>();
		}


		[Fact]
		public void TestValidApplicationAndMetrics()
		{
			// Arrange
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";
			IApplication mesosApp = _mesosFactory.Create(serviceName, taskIdentifier);
			HashSet<IApplication> applications = new HashSet<IApplication> {mesosApp};

			CloudStackService cloudStackService = GenerateCloudStackService();
			// Act
			cloudStackService.Poll();
			cloudStackService.Applications = applications;

			// Assert
			cloudStackService.Applications.Should().ContainSingle();
			cloudStackService.Applications.First().Should().Be(mesosApp);
			cloudStackService.Applications.First().Instances[0].Should().NotBeNull();
		}

		[Fact]
		public void TestMesosUpdateEventInstanceRunning()
		{
			// Arrange
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";
			IApplication mesosApp = _mesosFactory.Create(serviceName, taskIdentifier);

			CloudStackService cloudStackService = GenerateCloudStackService();
			cloudStackService.Applications = new HashSet<IApplication> {mesosApp};

			// Act
			Action act = () => cloudStackService.OnMesosUpdateEvent(null, new MesosUpdateEventArgs
			{
				Instance = new MesosInstance("123", InstanceState.Running, "123", "127.0.01", 80),
				Application = mesosApp,
				EventTypeType = ApplicationEventType.InstanceRunning,
				Identifier = "123"
			});

			// Assert
			act.Should().NotThrow();
		}

		[Fact]
		public void TestMesosUpdateEventInstanceStaging()
		{
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";
			IApplication mesosApp = _mesosFactory.Create(serviceName, taskIdentifier);

			// Arrange
			CloudStackService cloudStackService = GenerateCloudStackService();
			cloudStackService.Applications = new HashSet<IApplication> {mesosApp};

			// Act
			Action act = () =>
				cloudStackService.OnMesosUpdateEvent(null, new MesosUpdateEventArgs
				{
					Instance = new MesosInstance("123", InstanceState.Staging, "123", "127.0.01", 80),
					Application = mesosApp,
					EventTypeType = ApplicationEventType.InstanceStaging,
					Identifier = "123"
				});

			// Assert
			act.Should().NotThrow();
		}

		[Fact]
		public void TestUpdateEventInstanceRemoved()
		{
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";
			IApplication mesosApp = _mesosFactory.Create(serviceName, taskIdentifier);
			IInstance mesosInstance = mesosApp.Instances.First();

			// Arrange
			CloudStackService cloudStackService = GenerateCloudStackService();
			cloudStackService.Applications = new HashSet<IApplication> {mesosApp};

			// Act
			Action act = () =>
				cloudStackService.OnMesosUpdateEvent(null, new MesosUpdateEventArgs
				{
					Instance = mesosInstance,
					Application = mesosApp,
					EventTypeType = ApplicationEventType.InstanceRemoved,
					Identifier = mesosInstance.Identifier
				});

			// Assert
			act.Should().NotThrow();
			cloudStackService.Applications.Should().BeEmpty();
		}


		[Fact]
		public void TestMesosUpdateEventServiceAdded()
		{
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";
			IApplication mesosApp = _mesosFactory.Create(serviceName, taskIdentifier);

			// Arrange
			CloudStackService cloudStackService = GenerateCloudStackService();

			// Act
			Action act = () =>
				cloudStackService.OnMesosUpdateEvent(null, new MesosUpdateEventArgs
				{
					Instance = null,
					Application = mesosApp,
					EventTypeType = ApplicationEventType.ServiceAdded,
					Identifier = "123"
				});

			// Assert
			act.Should().NotThrow();
			cloudStackService.Applications.Should().ContainSingle().Subject.Name.Should().Be(serviceName);
		}

		[Fact]
		public void TestMesosUpdateEventServiceRemoved()
		{
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";
			IApplication mesosApp = _mesosFactory.Create(serviceName, taskIdentifier);

			// Arrange
			CloudStackService cloudStackService = GenerateCloudStackService();

			// Act
			Action act = () =>
				cloudStackService.OnMesosUpdateEvent(null, new MesosUpdateEventArgs
				{
					Instance = null,
					Application = mesosApp,
					EventTypeType = ApplicationEventType.ServiceRemoved,
					Identifier = "123"
				});

			// Assert
			act.Should().NotThrow();
			cloudStackService.Applications.Should().BeEmpty();
		}

		private CloudStackService GenerateCloudStackService()
		{
			// Arrange
			ApiOptions apiOptions = new ApiOptions
			{
				PrometheusEndpoint = "http://notexisting.url",
				MesosEndpoint = "http://notexisting.url",
				MarathonEndpoint = "http://notexisting.url"
			};
			Mock<IOptions<ApiOptions>> iOptionsMock = new Mock<IOptions<ApiOptions>>();
			iOptionsMock.Setup(x => x.Value).Returns(apiOptions);

			Mock<IApplicationProvider> mesosCollectorMock = new Mock<IApplicationProvider>();
			Mock<IMetricProvider> metricsCollectorMock = new Mock<IMetricProvider>();
			Mock<IApplicationProvider> marathonCollectorMock = new Mock<IApplicationProvider>();


			List<IMetric> metrics = new List<IMetric>();
			const string containerIdentifier = "blaze-test-container";
			IMetric metric = new Metric();
			IInstance instance = new MesosInstance(containerIdentifier, InstanceState.Running, containerIdentifier);
			metric.Values.Add("cpu_percentage", 1.0f);
			metrics.Add(metric);
			mesosCollectorMock.Setup(x => x.PollData());
			marathonCollectorMock.Setup(x => x.PollData());

			iOptionsMock.Setup(x => x.Value).Returns(apiOptions);

			return new CloudStackService(iOptionsMock.Object)
			{
				MesosCollector = mesosCollectorMock.Object,
				PrometheusCollector = metricsCollectorMock.Object,
				MarathonCollector = marathonCollectorMock.Object
			};
		}
	}
}