using System.Collections.Generic;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events;
using Blaze.SimTainer.Service.Providers.CloudStack.Services;
using Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Factories;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using FluentAssertions;
using Xunit;

namespace Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Services
{
	public class TestMesosUpdateHandlerServiceFixture
	{
		private MesosFactory _mesosFactory;

		public TestMesosUpdateHandlerServiceFixture()
		{
			_mesosFactory = new MesosFactory();
		}

		[Fact]
		public void TestUpdateHandler()
		{
			// Arrange
			MesosUpdateHandlerService mesosUpdateHandlerService = new MesosUpdateHandlerService();

			mesosUpdateHandlerService.HandleUpdates(new MesosEvent
			{
				AddTask = new MesosEventTaskAdd(),
				Type = MesosEventType.TASK_ADDED,
				UpdateTask = null
			});
			// Act & Assert
		}

		[Fact]
		public void TestMesosSubcriptionConverter()
		{
			// Arrange
			MesosUpdateHandlerService mesosUpdateHandlerService = new MesosUpdateHandlerService();

			MesosSubscription mesosSubscription = _mesosFactory.GenerateMesosSubscription(2, MesosTaskType.TASK_RUNNING);

			// Act

			HashSet<IApplication> applications = mesosUpdateHandlerService.ConvertMesosSubscriptions(mesosSubscription);

			// Assert
			applications.Should().HaveCount(2);
		}
	}
}