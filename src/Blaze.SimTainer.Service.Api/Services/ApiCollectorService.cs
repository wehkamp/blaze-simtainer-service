using Blaze.SimTainer.Service.Api.Dtos.Game;
using Blaze.SimTainer.Service.Api.EventHubs;
using Blaze.SimTainer.Service.Api.Interfaces;
using Blaze.SimTainer.Service.Providers.CloudStack.Services;
using Blaze.SimTainer.Service.Providers.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;

namespace Blaze.SimTainer.Service.Api.Services
{
	/// <summary>
	/// The main part of the application happens here.
	/// This class is a hosted service (Singleton) and keeps polling all Api's every 30 seconds.
	/// </summary>
	internal class ApiCollectorService : IHostedService
	{
		private readonly List<IProvider> _providers = new List<IProvider>();
		private readonly CloudStackDataConverterService _cloudStackDataConverterService;
		private readonly ILogger<ApiCollectorService> _logger;
		private readonly IHubContext<CloudStackGameEventHub, IGameEventHub> _gameHubContext;
		private Task _backgroundTask;
		private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();

		public ApiCollectorService(ILogger<ApiCollectorService> logger, CloudStackService cloudStackService,
			IHubContext<CloudStackGameEventHub, IGameEventHub> gameHubContext)
		{
			// Add provider to list of providers
			_providers.Add(cloudStackService);
			_cloudStackDataConverterService = new CloudStackDataConverterService();
			_logger = logger;
			_gameHubContext = gameHubContext;

			// Attach to the update event of the cloud stack service
			cloudStackService.UpdateEvent += CloudStackServiceOnUpdateEvent;
		}

		/// <summary>
		/// This function will generate an update event in the data converter service.
		/// After the update event is generated, the update will be send to all connected clients.
		/// </summary>
		/// <param name="sender">Can be null, is not used</param>
		/// <param name="updateEvent">Must be an update event containing all new information</param>
		private void CloudStackServiceOnUpdateEvent(object? sender, UpdateEvent updateEvent)
		{
			JsonSerializerSettings serializerSettings = new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
			try
			{
				UpdateEventDto updateEventDto = _cloudStackDataConverterService.GenerateUpdateEvent(updateEvent);
				if (updateEventDto != null)
					_gameHubContext.Clients.All.SendUpdateToClients(
						JsonConvert.SerializeObject(updateEventDto, serializerSettings));
			}
			catch (NullReferenceException e)
			{
				Debug.WriteLine($"We received an invalid object. Skipping update {e.Message}");
			}
		}

		/// <summary>
		/// Main task of the application, polling all API's from every provider that exists
		/// </summary>
		/// <returns></returns>
		private async Task Poll()
		{
			_logger.LogInformation("Starting to poll API's");
			while (!_shutdown.IsCancellationRequested)
			{
				foreach (IProvider provider in _providers)
				{
					provider.Poll();
				}

				await Task.Delay(TimeSpan.FromSeconds(30));
			}
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_backgroundTask = Task.Run(Poll, cancellationToken);

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_shutdown.Cancel();
			return Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
		}
	}
}