using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Models;

[assembly: InternalsVisibleTo("Blaze.SimTainer.Service.Api.Integration.UnitTests")]
namespace Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Factories
{
	internal class MesosFactory
	{
		/// <summary>
		/// This function will create a valid MesosApp based on the given parameters. Cpu, disks, gpus are always random. Version is the date of today.
		/// This will also create an instance
		/// </summary>
		/// <param name="serviceName"></param>
		/// <param name="taskIdentifier"></param>
		/// <param name="instanceState"></param>
		/// <returns></returns>
		public IApplication Create(string serviceName, string taskIdentifier, InstanceState instanceState = InstanceState.Running)
		{
			Random random = new Random();
			IApplication application = new MesosApplication
			{
				Name = serviceName,
				// Example identifier: test-service.mesos-29323823
				Identifier = taskIdentifier,
				Instances = new List<IInstance>
					{{new MesosInstance(taskIdentifier, instanceState, taskIdentifier, "127.0.0.1", 80)}},
				Type = "mesos",
				Role = null,
				Memory = 500,
				Team = "ccoe",
				Cpu = 0.1,
				Version = DateTime.Now
			};
			return application;
		}


		/// <summary>
		/// This method returns a list of mesos apps with the name being the index of the list
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public List<IApplication> CreateList(int amount)
		{
			List<IApplication> list = new List<IApplication>();
			for (int i = 0; i < amount; i++)
			{
				string identifier = i.ToString();
				list.Add(Create(identifier,identifier));
			}

			return list;
		}

		internal MesosTask GenerateMesosTask(string identifier, string teamName, MesosTaskType taskType)
		{
			return new MesosTask
			{
				State = taskType,
				Name = identifier,
				Identifier = new MesosTaskIdentifier {Value = $"marathon.{identifier}"},
				Resources = GenerateMesosResources(100, 0.1),
				Labels = GenerateMesosLabels(teamName),
				Statuses = GenerateMesosStatuses(identifier),
				Container = GenerateMesosContainer(80)
			};
		}

		internal List<MesosResource> GenerateMesosResources(int mem, double cpu)
		{
			return new List<MesosResource>()
			{
				new MesosResource
				{
					Name = "mem",
					Scalar = new MesosScalar
					{
						Value = mem
					},
				},
				new MesosResource
				{
					Name = "cpus",
					Scalar = new MesosScalar
					{
						Value = cpu
					},
				}
			};
		}
		internal MesosLabels GenerateMesosLabels(string teamName)
		{
			return new MesosLabels
			{
				Labels = new List<MesosLabel>
				{
					new MesosLabel
					{
						Key = "team",
						Value = teamName,
					}
				}
			};
		}
		internal List<MesosStatus> GenerateMesosStatuses(string identifier)
		{
			return new List<MesosStatus>()
			{
				new MesosStatus
				{
					Name = $"{identifier}",
					Identifier = $"{identifier}",
					State = MesosTaskType.TASK_RUNNING,

					ContainerStatus = new MesosContainerStatus
					{
						ContainerDictionary = new Dictionary<string, string>
						{
							{"value", "test"}
						},
						NetworkInfo = new List<MesosContainerNetworkInfo>()
						{
							new MesosContainerNetworkInfo
							{
								IpAddressDictionary = new List<MesosContainerIpAddress>()
								{
									new MesosContainerIpAddress()
									{
										IpAddress = "127.0.0.1"
									}
								}
							}
						}
					},
					MesosTaskIdentifier = new MesosTaskIdentifier
					{
						Value = $"marathon.{identifier}"
					}
				}
			};
		}
		internal MesosContainer GenerateMesosContainer(int port)
		{
			return new MesosContainer
			{
				DockerContainer = new MesosDockerContainer
				{
					PortMappingDictionary = new List<MesosDockerPortMapping>()
					{
						new MesosDockerPortMapping()
						{
							HostPort = port
						}
					}
				}
			};
		}

		internal MesosSubscription GenerateMesosSubscription(int amount, MesosTaskType taskType)
		{
			List<MesosTask> tasks = new List<MesosTask>();
			for (int i = 0; i < amount; i++)
			{
				tasks.Add(GenerateMesosTask($"{i}", "test-team", taskType));
			}
			MesosSubscription mesosSubscription = new MesosSubscription
			{
				MesosSubscribed = new MesosSubscribed
				{
					State = new MesosState
					{
						MesosGetTasks = new MesosGetTask
						{
							Tasks = tasks
						}
					}
				}
			};

			return mesosSubscription;
		}
	}
}