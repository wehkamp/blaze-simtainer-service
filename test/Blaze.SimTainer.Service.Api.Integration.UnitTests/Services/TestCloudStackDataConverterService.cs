using System;
using Blaze.SimTainer.Service.Api.Dtos.Game;
using Blaze.SimTainer.Service.Api.Dtos.Game.Layers;
using Blaze.SimTainer.Service.Api.Services;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Prometheus;
using Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Factories;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis;
using Blaze.SimTainer.Service.Providers.Shared.Models;
using Xunit;

namespace Blaze.SimTainer.Service.Api.Integration.UnitTests.Services
{
	public class TestCloudStackDataConverterService
	{
		private readonly MesosFactory _mesosFactory;
		private readonly PrometheusFactory _prometheusFactory;

		public TestCloudStackDataConverterService()
		{
			_mesosFactory = new MesosFactory();
			_prometheusFactory = new PrometheusFactory();
		}

		[Fact]
		public void TestCloudStackDataConverterServiceValidGameDto()
		{
			// Arrange
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";

			CloudStackDataConverterService cloudStackDataConverterService = new CloudStackDataConverterService();
			IApplication mesosApp = _mesosFactory.Create(serviceName, taskIdentifier);
			List<IApplication> applications = new List<IApplication> { mesosApp };
			PrometheusResult metricResult1 = _prometheusFactory.Create(mesosApp, taskIdentifier);

			applications[0].Instances[0].Metrics = metricResult1;
			// Act
			GameDto result = cloudStackDataConverterService.GenerateGameDto(applications);

			// Assert
			NeighbourhoodDto resultNeighbourhood = result.Neighbourhoods.First();

			resultNeighbourhood.Name.Should().Be(mesosApp.Name);
			resultNeighbourhood.VisualizedObjects.Should().HaveCount(1);
			resultNeighbourhood.VisualizedObjects[0].Type.Should().Be("building");
			resultNeighbourhood.VisualizedObjects[0].Size.Should().Be(5);
			resultNeighbourhood.DaysOld.Should().Be(0);

			resultNeighbourhood.LayerValues.Single(x => x.LayerType == "cpuLayer").Should().BeOfType<LayerValueDto>();
			resultNeighbourhood.LayerValues.Should().Contain(x => x.LayerType == "cpuLayer");
			resultNeighbourhood.LayerValues.Should().Contain(x => x.LayerType == "memoryLayer");
			resultNeighbourhood.LayerValues.Should().Contain(x => x.LayerType == "failedRequestsLayer");

			resultNeighbourhood.LayerValues.Should().HaveCount(3);
			resultNeighbourhood.VisualizedObjects[0].Identifier.Should().Be(taskIdentifier);

			// Assert layers are there as well
			result.Layers.Should().Contain(x => x.LayerType == "cpuLayer");
			result.Layers.Should().Contain(x => x.LayerType == "memoryLayer");
			result.Layers.Should().Contain(x => x.LayerType == "failedRequestsLayer");
			result.Layers.Should().HaveCount(3);
		}

		[Fact]
		public void TestCloudStackDataConverterServiceValidGameDtoMultiple()
		{
			// Arrange
			CloudStackDataConverterService cloudStackDataConverterService = new CloudStackDataConverterService();
			List<IApplication> applications = _mesosFactory.CreateList(2);

			PrometheusResult metricResult1 = _prometheusFactory.Create(applications[0], "test");
			PrometheusResult metricResult2 = _prometheusFactory.Create(applications[1], "test2");
			applications[0].Instances[0].Metrics = metricResult1;
			applications[1].Instances[0].Metrics = metricResult2;
			// Act
			GameDto result = cloudStackDataConverterService.GenerateGameDto(applications);

			// Assert
			result.Neighbourhoods.Should().HaveCount(2);
			result.Neighbourhoods.Single(x=>x.Name == applications[0].Name).VisualizedObjects[0].Type.Should().Be("building");
			result.Neighbourhoods.Single(x => x.Name == applications[0].Name).VisualizedObjects[0].Size.Should().Be(5);
			result.Neighbourhoods.Single(x => x.Name == applications[0].Name).VisualizedObjects[0].Identifier.Should().Be("0");
			result.Neighbourhoods.Single(x => x.Name == applications[1].Name).VisualizedObjects[0].Identifier.Should().Be("1");

			for (int i = 0; i < applications.Count; i++)
			{
				result.Neighbourhoods.Single(x => x.Name == applications[i].Name).Name.Should().Be(applications[i].Name);
			}
		}


		[Fact]
		public void TestGenerateUpdateEventServiceRemoved()
		{
			// Arrange
			CloudStackDataConverterService cloudStackDataConverterService = new CloudStackDataConverterService();
			const string serviceName = "test-service";
			string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";

			UpdateEvent updateEvent = new UpdateEvent();
			IApplication application = _mesosFactory.Create(serviceName, taskIdentifier);

			
			updateEvent.Application = application;
			updateEvent.ApplicationEventType = ApplicationEventType.ServiceRemoved;
			// Act
			UpdateEventDto result = cloudStackDataConverterService.GenerateUpdateEvent(updateEvent);

			// Assert
			result.RemovedNeighbourhood.Should().Be(application.Name);
			result.RemovedNeighbourhood.Should().NotBe(application.Identifier);
		}

		[Fact]
		public void TestGenerateUpdateEventServiceAdded()
		{
			// Arrange
			CloudStackDataConverterService cloudStackDataConverterService = new CloudStackDataConverterService();
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";

			UpdateEvent updateEvent = new UpdateEvent();
			IApplication application = _mesosFactory.Create(serviceName, taskIdentifier);

			updateEvent.Application = application;
			updateEvent.ApplicationEventType = ApplicationEventType.ServiceAdded;
			// Act
			UpdateEventDto result = cloudStackDataConverterService.GenerateUpdateEvent(updateEvent);

			// Assert
			result.AddedNeighbourhood.Name.Should().Be(application.Name);
			result.AddedNeighbourhood.Team.Should().Be(application.Team);
			result.AddedNeighbourhood.DaysOld.Should().Be((DateTime.Now - application.Version).Days);
			result.AddedNeighbourhood.LayerValues.Should().Contain(x => x.LayerType == "cpuLayer");
			result.AddedNeighbourhood.LayerValues.Should().Contain(x => x.LayerType == "memoryLayer");
			result.AddedNeighbourhood.VisualizedObjects.Should().ContainSingle();
			result.NeighbourhoodName.Should().Be(application.Name);
		}

		[Fact]
		public void TestGenerateUpdateEventInstanceStaging()
		{
			// Arrange
			CloudStackDataConverterService cloudStackDataConverterService = new CloudStackDataConverterService();
			const string serviceName = "test-service";
			const string taskIdentifier = "0f8fad5b-d9cb-469f-a165-70867728950e";
			const string containerIdentifier = "test-service-container-1.test123";

			UpdateEvent updateEvent = new UpdateEvent();
			IApplication application = _mesosFactory.Create(serviceName, taskIdentifier);

			IInstance instance = new MesosInstance(containerIdentifier, InstanceState.Staging, taskIdentifier, taskIdentifier);

			updateEvent.Application = application;
			updateEvent.Instance = instance;

			updateEvent.ApplicationEventType = ApplicationEventType.InstanceStaging;
			// Act
			UpdateEventDto result = cloudStackDataConverterService.GenerateUpdateEvent(updateEvent);

			// Assert
			result.AddedVisualizedObject.Type.Should().Be("staging-building");
			result.AddedVisualizedObject.Identifier.Should().Be(taskIdentifier);
		}
	}
}