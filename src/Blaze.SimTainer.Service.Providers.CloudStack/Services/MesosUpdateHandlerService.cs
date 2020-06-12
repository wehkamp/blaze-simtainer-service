using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Events;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Services
{
	internal class MesosUpdateHandlerService
	{
		public event EventHandler<MesosInitializationEventArgs> MesosInitializationEvent;
		public event EventHandler<MesosUpdateEventArgs> MesosUpdateEvent;

		/// <summary>
		/// Property that is being set when this service should subscribe to a specific framework name.
		/// </summary>
		internal MesosFrameworkInfo SelectedFramework { get; set; }

		/// <summary>
		/// The actual name of the selected framework should be set, in case of framework updates (add, update or remove).
		/// </summary>
		internal string SelectedFrameworkName { get; set; }

		public MesosUpdateHandlerService(string selectedFrameworkName)
		{
			SelectedFrameworkName = selectedFrameworkName;
		}

		/// <summary>
		/// Function to handle updates that we receive from mesos <see cref="ApiCollectors.MesosCollector"/>
		/// </summary>
		/// <param name="mesosEvent"></param>
		internal void HandleUpdates(MesosEvent mesosEvent)
		{
			// We don't need to check every event
			switch (mesosEvent.Type)
			{
				case MesosEventType.TASK_ADDED:

					if (!string.IsNullOrEmpty(SelectedFrameworkName) &&
					    (SelectedFramework == null || mesosEvent.AddTask.Task.FrameworkIdentifier.Value !=
						    SelectedFramework.Identifier.Value))
					{
						// Unhandled event
						return;
					}

					MesosTask mesosTask = mesosEvent.AddTask.Task;
					MesosUpdateEvent?.Invoke(this,
						new MesosUpdateEventArgs
						{
							Application = GenerateApplication(mesosTask),
							Identifier = mesosTask.Name,
							EventTypeType = ApplicationEventType.ServiceAdded,
							Instance = new MesosInstance(mesosTask.Identifier.TaskIdentifier,
								InstanceState.Unknown, mesosTask.Identifier.TaskIdentifier,
								mesosTask.Identifier.Value, null, mesosTask.Container.DockerContainer.HostPort)
						});
					break;
				case MesosEventType.TASK_UPDATED:
					if (!string.IsNullOrEmpty(SelectedFrameworkName) &&
					    (SelectedFramework == null || mesosEvent.UpdateTask.FrameworkIdentifier.Value !=
						    SelectedFramework.Identifier.Value))
					{
						// Unhandled event
						return;
					}
					switch (mesosEvent.UpdateTask.State)
					{
						// A lot of cases, but with all these cases, the instance should be removed
						case MesosTaskType.TASK_GONE_BY_OPERATOR:
						case MesosTaskType.TASK_GONE:
						case MesosTaskType.TASK_LOST:
						case MesosTaskType.TASK_ERROR:
						case MesosTaskType.TASK_FINISHED:
						case MesosTaskType.TASK_KILLED:
						case MesosTaskType.TASK_FAILED:
						case MesosTaskType.TASK_KILLING:
						case MesosTaskType.TASK_DROPPED:
						case MesosTaskType.TASK_UNKNOWN:
						case MesosTaskType.TASK_UNREACHABLE:
							MesosUpdateEvent?.Invoke(mesosEvent,
								new MesosUpdateEventArgs
								{
									Identifier = mesosEvent.UpdateTask.Status.MesosTaskIdentifier.TaskIdentifier,
									EventTypeType = ApplicationEventType.InstanceRemoved
								});
							break;
						case MesosTaskType.TASK_STAGING:
						case MesosTaskType.TASK_STARTING:
							MesosUpdateEvent?.Invoke(mesosEvent,
								new MesosUpdateEventArgs
								{
									Instance = new MesosInstance(
										mesosEvent.UpdateTask.Status.MesosTaskIdentifier.TaskIdentifier,
										InstanceState.Staging,
										mesosEvent.UpdateTask.Status.MesosTaskIdentifier.TaskIdentifier,
										mesosEvent.UpdateTask.Status.MesosTaskIdentifier.Value,
										mesosEvent.UpdateTask.Status.ContainerStatus.IpAddress),
									Identifier = mesosEvent.UpdateTask.Status.MesosTaskIdentifier.ServiceName,
									EventTypeType = ApplicationEventType.InstanceStaging
								});
							break;
						case MesosTaskType.TASK_RUNNING:
							MesosUpdateEvent?.Invoke(mesosEvent,
								new MesosUpdateEventArgs
								{
									Instance = new MesosInstance(
										mesosEvent.UpdateTask.Status.ContainerStatus.ContainerId,
										InstanceState.Running,
										mesosEvent.UpdateTask.Status.MesosTaskIdentifier.TaskIdentifier,
										mesosEvent.UpdateTask.Status.MesosTaskIdentifier.Value,
										mesosEvent.UpdateTask.Status.ContainerStatus.IpAddress),
									Identifier = mesosEvent.UpdateTask.Status.MesosTaskIdentifier.ServiceName,
									EventTypeType = ApplicationEventType.InstanceRunning
								});
							break;
						default:
							Console.WriteLine($"Task not handled! {mesosEvent.UpdateTask.State}");
							break;
					}

					break;
				case MesosEventType.FRAMEWORK_ADDED:
					// Framework has probably been re-created with a new identifier. So set the new selected framework
					if (!string.IsNullOrEmpty(SelectedFrameworkName) &&
					    mesosEvent.AddFramework.FrameworkInfo.Name == SelectedFrameworkName)
					{
						Console.WriteLine(
							$"[MesosUpdateHandlerService] Selected framework is added with identifier {mesosEvent.AddFramework.FrameworkInfo.Identifier.Value}");
						SelectedFramework = mesosEvent.AddFramework.FrameworkInfo;
					}

					break;
				case MesosEventType.FRAMEWORK_UPDATED:
					if (!string.IsNullOrEmpty(SelectedFrameworkName) &&
					    mesosEvent.UpdatedFramework.FrameworkInfo.Name == SelectedFrameworkName)
					{
						Console.WriteLine(
							$"[MesosUpdateHandlerService] Selected framework is updated with identifier {mesosEvent.UpdatedFramework.FrameworkInfo.Identifier.Value}");
						SelectedFramework = mesosEvent.UpdatedFramework.FrameworkInfo;
					}

					break;
				case MesosEventType.FRAMEWORK_REMOVED:
					if (!string.IsNullOrEmpty(SelectedFrameworkName) &&
					    mesosEvent.RemovedFramework.FrameworkInfo.Name == SelectedFrameworkName)
					{
						Console.WriteLine(
							$"[MesosUpdateHandlerService] Selected framework is removed with identifier {mesosEvent.RemovedFramework.FrameworkInfo.Identifier.Value}");
						SelectedFramework = null;
					}

					break;
			}
		}

		/// <summary>
		/// Function that should be used when a Mesos subscription is received to initialize the applications.
		/// </summary>
		/// <param name="mesosSubscription"></param>
		internal void InitializeMesosSubscription(MesosSubscription mesosSubscription)
		{
			HashSet<IApplication> applications = ConvertMesosSubscriptions(mesosSubscription);


			// Set the framework that is selected to listen to
			if (!string.IsNullOrEmpty(SelectedFrameworkName))
			{
				SelectedFramework = mesosSubscription.MesosSubscribed.State.MesosGetFrameworks.Frameworks.Single(x =>
					x.FrameworkInfo.Name == SelectedFrameworkName).FrameworkInfo;
			}

			MesosInitializationEvent?.Invoke(this,
				new MesosInitializationEventArgs {Applications = applications});
		}

		/// <summary>
		/// Function to convert mesos subscriptions to a list of applications
		/// </summary>
		/// <param name="mesosSubscription"></param>
		/// <returns></returns>
		internal HashSet<IApplication> ConvertMesosSubscriptions(MesosSubscription mesosSubscription)
		{
			// Get all running tasks, because we don't need killed tasks at this point
			List<MesosTask> runningTasks =
				mesosSubscription.MesosSubscribed.State.MesosGetTasks.Tasks.Where(x =>
					x.State == MesosTaskType.TASK_RUNNING).OrderBy(x => x.Name).ToList();
			HashSet<IApplication> applications = new HashSet<IApplication>();

			foreach (MesosTask mesosTask in runningTasks)
			{
				IApplication newApplication = applications.SingleOrDefault(x => x.Name == mesosTask.Name);
				if (newApplication == null)
				{
					newApplication = GenerateApplication(mesosTask);
					applications.Add(newApplication);
				}

				// We need the IP address later for the metrics
				string ipAddress = mesosTask.Statuses.SingleOrDefault(x => x.State == MesosTaskType.TASK_STARTING)?
					.ContainerStatus.IpAddress;
				MesosStatus mesosStatus =
					mesosTask.Statuses.SingleOrDefault(x => x.State == MesosTaskType.TASK_RUNNING);
				if (mesosStatus != null)
				{
					newApplication.Instances.Add(new MesosInstance(mesosStatus.ContainerStatus.ContainerId,
						mesosStatus.InstanceState, mesosStatus.MesosTaskIdentifier.TaskIdentifier,
						mesosStatus.MesosTaskIdentifier.Value, ipAddress,
						mesosTask.Container.DockerContainer.HostPort));
				}
			}

			return applications;
		}


		/// <summary>
		/// Function to generate an application based on a mesos task
		/// </summary>
		/// <param name="mesosTask"></param>
		/// <returns></returns>
		private static IApplication GenerateApplication(MesosTask mesosTask)
		{
			IApplication newApplication = new MesosApplication
			{
				Name = mesosTask.Name,
				Memory = mesosTask.Resources.Single(x => x.Name == "mem").Scalar.Value,
				Cpu = mesosTask.Resources.Single(x => x.Name == "cpus").Scalar.Value,
				Team = mesosTask.Labels?.Labels.SingleOrDefault(x => x.Key == "team")?.Value,
				Identifier = mesosTask.Identifier.Value
			};
			return newApplication;
		}
	}
}