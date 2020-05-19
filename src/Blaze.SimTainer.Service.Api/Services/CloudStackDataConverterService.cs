using Blaze.SimTainer.Service.Api.Dtos.Game;
using Blaze.SimTainer.Service.Api.Dtos.Game.Layers;
using Blaze.SimTainer.Service.Api.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Blaze.SimTainer.Service.Api.Services
{
	internal class CloudStackDataConverterService : IDataConverter<IApplication>
	{
		private readonly IVisualLayer[] _visualLayers = {new CpuLayer(), new MemoryLayer(), new FailedRequestsLayer()};

		/// <summary>
		/// This function will generate a fresh game data transfer object.
		/// </summary>
		/// <param name="applications"></param>
		/// <returns></returns>
		public GameDto GenerateGameDto(ICollection<IApplication> applications)
		{
			IEnumerable<NeighbourhoodDto> neighbourhoods = applications.Select(GenerateNeighbourhoodDto);

				HashSet<string> teams = applications.Select(x => x.Team).Where(x => x != null).Distinct().ToHashSet();
			GameDto gameDto = new GameDto
				{Neighbourhoods = neighbourhoods.ToHashSet(), Layers = _visualLayers, Teams = teams};
			return gameDto;
		}

		/// <summary>
		/// Function to generate a neighbourhood based on a <see cref="IApplication"/>.
		/// It checks for metrics and created layer values. Also generate each vehicle or building.
		/// </summary>
		/// <param name="application"></param>
		/// <returns></returns>
		private NeighbourhoodDto GenerateNeighbourhoodDto(IApplication application)
		{
			List<LayerValueDto> layerValues = new List<LayerValueDto>();
			// Add the CPU Layer
			LayerValueDto cpuLayer = new LayerValueDto
				{LayerType = LayerTypeEnum.cpuLayer.ToString(), MinValue = 0, MaxValue = 100};
			// Add the memory layer
			LayerValueDto memoryLayer = new LayerValueDto
				{LayerType = LayerTypeEnum.memoryLayer.ToString(), MinValue = 0, MaxValue = application.Memory};
			try
			{
				// Try to add the failed requests layer. Not every container provides network metrics
				LayerValueDto failedRequestsLayer = new LayerValueDto
				{
					LayerType = LayerTypeEnum.failedRequestsLayer.ToString(),
					MinValue = 0,
					MaxValue = application.Instances
						.Where(instance => instance.Metrics != null)
						.Max(instance =>
							instance.Metrics.Values.ContainsKey("network_requests_failed")
								? instance.Metrics.Values["network_requests_failed"]
								: 0)
				};
				layerValues.Add(failedRequestsLayer);
			}
			catch (Exception e)
			{
				// Failed requests do not exists, so we skip this layer value
			}
			// Add the layers to the list
			layerValues.Add(cpuLayer);
			layerValues.Add(memoryLayer);
			
			// Create a list for the visualized objects that we are going to add
			List<IVisualizedObjectDto> objects = new List<IVisualizedObjectDto>();

			foreach (IInstance instance in application.Instances)
			{
				// Every instance is a building, so generate a building DTO
				IVisualizedObjectDto building = GenerateBuildingDto(application, instance, false);
				objects.Add(building);
				if (instance.Metrics != null)
				{
					// If we have metrics, try to generate a vehicle DTO (network metrics)
					IVisualizedObjectDto vehicleDto =
						GenerateVehicleDto(instance.Identifier, instance.Metrics.Values);
					if (vehicleDto != null)
					{
						objects.Add(vehicleDto);
					}
				}
			}

			int daysOld = 0;

			// Check this because some services have no valid datetime due to they are being unknown to marathon
			if (application.Version.Year > 2000)
			{
				daysOld = (DateTime.Now - application.Version).Days;
			}

			return new NeighbourhoodDto
			{
				Team = application.Team,
				Name = application.Name,
				VisualizedObjects = objects,
				LayerValues = layerValues,
				DaysOld = daysOld
			};
		}

		/// <summary>
		/// Function to generate a vehicle based on network requests metrics.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="metricValues"></param>
		/// <returns></returns>
		private IVisualizedObjectDto GenerateVehicleDto(string identifier, Dictionary<string, double> metricValues)
		{
			if (!metricValues.ContainsKey("network_requests_success")) return null;
			VisualizedVehicleDto visualizedVehicle = new VisualizedVehicleDto
			{
				Identifier = identifier,
				Size = Convert.ToInt32(metricValues["network_requests_success"])
			};
			return visualizedVehicle;
		}

		private IVisualizedObjectDto GenerateBuildingDto(IApplication application, IInstance instance,
			bool staging)
		{
			IVisualizedObjectDto building;
			if (!staging)
			{
				building = new VisualizedBuildingDto();
			}
			else
			{
				building = new VisualizedStagingBuildingDto();
			}

			// Fill the metrics if they are available
			if (instance.Metrics != null)
				building.LayerValues = ConvertMetricsToLayerValues(instance.Metrics.Values);

			double size = Convert.ToDouble(application.Memory) / 10 * application.Cpu;
			building.Size = (int) Math.Ceiling(size);
			building.Identifier = instance.Identifier;
			return building;
		}

		/// <summary>
		/// This function will convert all metrics to layer values. It needs a dictionary with all the values.
		/// </summary>
		/// <param name="metricsDictionary">Expected keys for now are cpu_usage, memory_usage and network_requests_failed with expected type double</param>
		/// <returns></returns>
		private Dictionary<string, double> ConvertMetricsToLayerValues(Dictionary<string, double> metricsDictionary)
		{
			Dictionary<string, double> newDictionary = new Dictionary<string, double>();
			double cpuUsage = 0;
			double memoryUsage = 0;
			double failedRequests = 0;
			// Check if there is a key for CPU usage
			if (metricsDictionary.ContainsKey("cpu_usage"))
				cpuUsage = metricsDictionary["cpu_usage"];
			// Check if there is a key for memory usage
			if (metricsDictionary.ContainsKey("memory_usage"))
				memoryUsage = metricsDictionary["memory_usage"];
			// Check if there is a key for failed network requests
			if (metricsDictionary.ContainsKey("network_requests_failed"))
				failedRequests = metricsDictionary["network_requests_failed"];

			// Just add all the values, even if it's 0. We want consistency. 
			newDictionary.Add(LayerTypeEnum.memoryLayer.ToString(),
				memoryUsage);
			newDictionary.Add(LayerTypeEnum.cpuLayer.ToString(), cpuUsage);
			newDictionary.Add(LayerTypeEnum.failedRequestsLayer.ToString(), failedRequests);
			return newDictionary;
		}

		/// <summary>
		/// This function will generate an update event if something is updated.
		/// </summary>
		/// <param name="updateEvent"></param>
		/// <returns></returns>
		public UpdateEventDto GenerateUpdateEvent(UpdateEvent updateEvent)
		{
			UpdateEventDto updateEventDto = new UpdateEventDto {NeighbourhoodName = updateEvent.Application?.Name};
			switch (updateEvent.ApplicationEventType)
			{
				// An instance is remove, the game only needs to know the identifier
				case ApplicationEventType.InstanceRemoved:
					if (updateEvent.Instance != null)
						updateEventDto.RemovedVisualizedObject = updateEvent.Instance.Identifier;
					else
					{
						Debug.WriteLine("Instance is null in update event!");
						return null;
					}

					break;
				case ApplicationEventType.InstanceRunning:
					// We passed the staging task, so there is still a staging-building active, we just set the RemovedVisualizedObject to the current identifier
					updateEventDto.RemovedVisualizedObject = updateEvent.Instance.Identifier;
					updateEventDto.AddedVisualizedObject =
						GenerateBuildingDto(updateEvent.Application,
							updateEvent.Instance, false);
					break;
				case ApplicationEventType.ServiceAdded:
					// New service is added, we generate a new neighbourhood dto
					if (updateEvent.Application != null)
						updateEventDto.AddedNeighbourhood =
							GenerateNeighbourhoodDto(updateEvent.Application);
					else
						return null;
					break;
				// A service is removed, we only set the identifier of a service
				case ApplicationEventType.ServiceRemoved:
					updateEventDto.RemovedNeighbourhood = updateEvent.Application?.Name;
					break;
				// Metrics are updated, it will look like this:
				// {
				//    "IDENTIFIER of a container":
				//    {
				//     "value1": 0.1,
				//     "value2" 0.2
				//    }
				// }
				// With this we can update layer values individually
				case ApplicationEventType.MetricsUpdate:
					if (updateEvent.MetricValues != null)
					{
						Dictionary<string, Dictionary<string, double>> newDict =
							updateEvent.MetricValues.ToDictionary(metricValue => metricValue.Key,
								metricValue => ConvertMetricsToLayerValues(metricValue.Value));
						List<IVisualizedObjectDto> visualizedObjectDtos = new List<IVisualizedObjectDto>();
						updateEventDto.UpdatedLayerValues = newDict;
						foreach ((string key, Dictionary<string, double> value) in updateEvent.MetricValues)
						{
							IVisualizedObjectDto vehicle = GenerateVehicleDto(key, value);
							if (vehicle == null) continue;
							visualizedObjectDtos.Add(vehicle);
						}
						updateEventDto.UpdatedVisualizedObjects = visualizedObjectDtos;
					}
					break;
				// Instance is staging! A new container is born.
				case ApplicationEventType.InstanceStaging:
					updateEventDto.AddedVisualizedObject =
						GenerateBuildingDto(updateEvent.Application,
							updateEvent.Instance, true);
					break;
				// Service is updated, for example the age or team is changed.
				case ApplicationEventType.ServiceUpdated:
					updateEventDto.UpdatedNeighbourhood =
						GenerateNeighbourhoodDto(updateEvent.Application);
					break;
				// This should never happen
				default:
					throw new ArgumentOutOfRangeException();
			}

			return updateEventDto;
		}
	}
}