using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Marathon;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Events;
using Blaze.SimTainer.Service.Providers.CloudStack.Services.ApiCollectors;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Services
{
	/// <summary>
	/// This service gathers cloud stack data. This can be anything
	/// </summary>
	public class CloudStackService : IProvider
	{
		public IApplicationProvider MesosCollector;
		public IMetricProvider PrometheusCollector;
		public IApplicationProvider MarathonCollector;
		internal MesosUpdateHandlerService MesosUpdateHandlerService;
		public HashSet<IApplication> Applications = new HashSet<IApplication>();
		public event EventHandler<UpdateEvent> UpdateEvent;
		internal TaskKillService TaskKillService;

		public CloudStackService(IOptions<ApiOptions> apiOptions)
		{
			// Initialize services
			MesosUpdateHandlerService = new MesosUpdateHandlerService();
			TaskKillService = new TaskKillService(apiOptions.Value.MarathonEndpoint, new HttpClient());

			// Initialize collector services
			MesosCollector = new MesosCollector(apiOptions.Value.MesosEndpoint, new HttpClient(),
				MesosUpdateHandlerService);
			PrometheusCollector = new PrometheusCollector(apiOptions.Value.PrometheusEndpoint, new HttpClient());
			MarathonCollector = new MarathonCollector(apiOptions.Value.MarathonEndpoint, new HttpClient());

			// Initialize event handlers
			MesosUpdateHandlerService.MesosInitializationEvent +=
				OnMesosInitializationEvent;
			PrometheusCollector.PrometheusUpdateEvent += PrometheusCollectorOnPrometheusUpdateEvent;
			MesosUpdateHandlerService.MesosUpdateEvent += OnMesosUpdateEvent;
		}

		/// <summary>
		/// This function wil be called every time there is an update of mesos.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMesosUpdateEvent(object? sender, MesosUpdateEventArgs e)
		{
			UpdateEvent updateEvent = new UpdateEvent {ApplicationEventType = e.EventTypeType};

			IApplication app;
			switch (e.EventTypeType)
			{
				case ApplicationEventType.InstanceStaging:
					app = Applications.SingleOrDefault(x => x.Name == e.Identifier);
					MesosInstance eInstance = (MesosInstance) e.Instance;
					if (app == null)
						Debug.WriteLine("Application is null, which should never happen.");
					else
					{
						if (!app.Instances.Contains(eInstance))
							app.Instances.Add(e.Instance);
						else
							UpdateInstance(app.Instances[app.Instances.IndexOf(e.Instance)], e.Instance, true);
					}

					updateEvent.Application = app;
					updateEvent.Instance = e.Instance;
					break;
				case ApplicationEventType.InstanceRunning:
					app = Applications.SingleOrDefault(x => x.Name == e.Identifier);

					if (app == null)
						Debug.WriteLine("This should not happen. App does not exists");
					else
					{
						if (!app.Instances.Contains(e.Instance))
							app.Instances.Add(e.Instance);
						else
							UpdateInstance(app.Instances[app.Instances.IndexOf(e.Instance)], e.Instance);
					}

					updateEvent.Application = app;
					updateEvent.Instance = e.Instance;
					break;
				case ApplicationEventType.InstanceRemoved:
					IApplication applicationToRemove = null;
					foreach (IApplication application in Applications)
					{
						IInstance instanceToRemove =
							application.Instances.SingleOrDefault(x => x.Identifier == e.Identifier);
						if (instanceToRemove == null) continue;
						updateEvent.Instance = instanceToRemove;
						updateEvent.Application = application;
						application.Instances.Remove(instanceToRemove);

						// All instances are gone so a service is removed
						if (application.Instances.Count == 0)
						{
							applicationToRemove = application;
							updateEvent.ApplicationEventType = ApplicationEventType.ServiceRemoved;
						}

						break;
					}

					if (applicationToRemove != null)
						Applications.Remove(applicationToRemove);

					if (updateEvent.Instance == null && updateEvent.Application == null)
						return;
					break;
				case ApplicationEventType.ServiceAdded:
					app = Applications.SingleOrDefault(x => x.Name == e.Application.Name);

					if (app == null)
					{
						Applications.Add(e.Application);
						updateEvent.Application = Applications.Single(x => x.Name == e.Application.Name);

						// Updated version of every application
						UpdateApplicationVersions();
					}
					else
					{
						bool appUpdates = UpdateApp(app, e.Application);
						bool instanceUpdates = false;
						if (e.Instance != null)
						{
							if (!app.Instances.Contains(e.Instance))
							{
								app.Instances.Add(e.Instance);
								instanceUpdates = true;
							}
							else
								instanceUpdates = UpdateInstance(app.Instances[app.Instances.IndexOf(e.Instance)],
									e.Instance);

							updateEvent.Instance = app.Instances[app.Instances.IndexOf(e.Instance)];
						}

						// New updates so send an event
						if (appUpdates)
						{
							updateEvent.ApplicationEventType = ApplicationEventType.ServiceUpdated;
							updateEvent.Application = app;
						}

						else if (instanceUpdates)
						{
							updateEvent.ApplicationEventType = ApplicationEventType.InstanceStaging;
							updateEvent.Application = app;
						}
						else
						{
							updateEvent.Application = null;
							// Nothing updates so we will not trigger an event
							return;
						}
					}

					break;
				case ApplicationEventType.ServiceRemoved:
					app = Applications.SingleOrDefault(x => x.Identifier == e.Identifier);
					updateEvent.Application = app;
					Applications.Remove(app);
					break;
			}

			// Invoke event handlers
			UpdateMetrics();
			UpdateEvent?.Invoke(e, updateEvent);
		}

		/// <summary>
		/// Function to update all metrics of every running instance.
		/// </summary>
		internal void UpdateMetrics()
		{
			foreach (IInstance instance in Applications.SelectMany(application => application.Instances)
				.Where(instance => instance != null && instance.State == InstanceState.Running))
			{
				// Fill metrics for every instance
				instance.Metrics =
					PrometheusCollector.GetMetrics(instance);
			}
		}

		/// <summary>
		/// This function will be called everytime the prometheus metrics are updated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void PrometheusCollectorOnPrometheusUpdateEvent(object? sender, EventArgs e)
		{
			try
			{
				UpdateMetrics();
				Dictionary<string, Dictionary<string, double>> metricValues = Applications
					.Where(x => x?.Instances != null)
					.SelectMany(application => application.Instances)
					.Where(x => x.State == InstanceState.Running)
					.ToDictionary(instance => instance.Identifier, instance => instance.Metrics.Values);
				UpdateEvent updateEvent = new UpdateEvent
				{
					MetricValues = metricValues,
					ApplicationEventType = ApplicationEventType.MetricsUpdate
				};
				UpdateEvent?.Invoke(this, updateEvent);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Something went wrong with the metrics collection {ex.Message}");
			}
		}

		/// <summary>
		/// This event should only be called once. This only happens when the mesos data is initialized.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMesosInitializationEvent(object? sender,
			MesosInitializationEventArgs e)
		{
			Applications = e.Applications;
			// Update all metrics
			UpdateMetrics();
			// Update all versions of application
			UpdateApplicationVersions();
		}

		/// <summary>
		/// Update all versions of every application by the data of Marathon.
		/// </summary>
		internal void UpdateApplicationVersions()
		{
			if (!(MarathonCollector is MarathonCollector m)) return;
			foreach (IApplication application in Applications)
			{
				MarathonApp marathonApp =
					m.MarathonApps.SingleOrDefault(x => x.Name == application.Name);
				if (marathonApp != null)
				{
					application.Version = marathonApp.Version;
				}
			}
		}

		/// <summary>
		/// Main function of this service. This function should be called every x amount of time.
		/// </summary>
		public void Poll()
		{
			MarathonCollector.PollData();
			MesosCollector.PollData();
			PrometheusCollector.PollMetrics();
			Debug.WriteLine("Polled all data");
		}

		/// <summary>
		/// Update application with newer application.
		/// </summary>
		/// <param name="oldApplication"></param>
		/// <param name="newApplication"></param>
		/// <returns></returns>
		internal bool UpdateApp(IApplication oldApplication, IApplication newApplication)
		{
			bool updated = false;

			if (Math.Abs(oldApplication.Cpu - newApplication.Cpu) > 0.1)
			{
				oldApplication.Cpu = newApplication.Cpu;
				updated = true;
			}

			if (Math.Abs(oldApplication.Memory - newApplication.Memory) > 0.1)
			{
				oldApplication.Memory = newApplication.Memory;
				updated = true;
			}

			if (oldApplication.Team != newApplication.Team)
			{
				oldApplication.Team = newApplication.Team;
				updated = true;
			}

			return updated;
		}

		/// <summary>
		/// Update instance with new instance.
		/// </summary>
		/// <param name="oldInstance"></param>
		/// <param name="newInstance"></param>
		/// <param name="updateIp"></param>
		/// <returns></returns>
		internal bool UpdateInstance(IInstance oldInstance, IInstance newInstance, bool updateIp = false)
		{
			bool updated = false;

			// If the instance is a Mesos instance, we want to update the IP and port and container identifier
			if (oldInstance is MesosInstance oldInstance1 && newInstance is MesosInstance newInstance1)
			{
				if (updateIp)
				{
					if (newInstance1.Ip != null && oldInstance1.Ip != newInstance1.Ip)
					{
						oldInstance1.Ip = newInstance1.Ip;
						updated = true;
					}

					if (newInstance1.Port > 0 && oldInstance1.Port != newInstance1.Port)
					{
						oldInstance1.Port = newInstance1.Port;
						updated = true;
					}
				}

				if (newInstance1.ContainerIdentifier != oldInstance1.ContainerIdentifier)
				{
					oldInstance1.ContainerIdentifier = newInstance1.ContainerIdentifier;
				}
			}

			if (oldInstance.State != newInstance.State)
			{
				oldInstance.State = newInstance.State;
				updated = true;
			}

			return updated;
		}

		/// <summary>
		/// Function to kill a task
		/// </summary>
		/// <param name="taskIdentifier">Identifier of a task (container).</param>
		/// <param name="force">Set force to true if you want to skip the check if there are at least 2 containers running.</param>
		/// <returns></returns>
		public bool KillTask(string taskIdentifier, bool force)
		{
			var target = Applications
				.SelectMany(p => p.Instances,
					(application, instance) => new
						{Application = application, Instance = instance})
				.Where(x => x.Instance.State == InstanceState.Running)
				.SingleOrDefault(x => x.Instance.Identifier == taskIdentifier);

			if (target == null) return false;

			// We do not want to kill services with only 1 instance left, because that could cause serious issues
			if (target.Application.Instances.Count(x => x.State == InstanceState.Running) < 2 && !force) return false;


			if (!TaskKillService.KillTask(target.Application, target.Instance)) return false;

			// Quickly update the instance state to a state of unknown because we are not sure when it gets killed (task queue of mesos)
			// Plus in case 2 requests happen and all containers get destroyed
			target.Instance.State = InstanceState.Unknown;

			UpdateEvent updateEvent = new UpdateEvent
			{
				ApplicationEventType = ApplicationEventType.InstanceRemoved,
				Instance = target.Instance,
				Application = target.Application
			};
			target.Application.Instances.Remove(target.Instance);
			UpdateEvent?.Invoke(null, updateEvent);
			return true;
		}
	}
}