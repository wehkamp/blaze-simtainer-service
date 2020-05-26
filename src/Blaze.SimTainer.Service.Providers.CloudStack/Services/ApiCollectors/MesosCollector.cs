using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Services.ApiCollectors
{
	internal class MesosCollector : IApplicationProvider
	{
		private readonly string _baseUrl;

		private readonly HttpClient _httpClient;
		private readonly MesosUpdateHandlerService _mesosUpdateHandlerService;

		public MesosCollector(string baseUrl, HttpClient httpClient,
			MesosUpdateHandlerService mesosUpdateHandlerService)
		{
			_baseUrl = baseUrl;
			_httpClient = httpClient;
			_mesosUpdateHandlerService = mesosUpdateHandlerService;
		}

		public bool StartedListening { get; private set; }

		/// <summary>
		/// We poll all the data async and we send events to the <see cref="MesosUpdateHandlerService"/>
		/// </summary>
		public async void PollData()
		{
			// We don't want to start multiple pollers
			if (StartedListening)
			{
				return;
			}

			StartedListening = true;
			string url = _baseUrl + "api/v1";
			Uri uri;
			try
			{
				uri = new Uri(url);
			}
			catch (UriFormatException)
			{
				Console.WriteLine("[MesosCollector] Invalid Base URL");
				return;
			}
			Console.WriteLine("[MesosCollector] Starting to listen to Mesos Events");

			const string content = "{\"type\":\"SUBSCRIBE\"}";
			int count = 0;
			_httpClient.DefaultRequestHeaders
				.Accept
				.Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header

			_httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

			HttpRequestMessage request = new HttpRequestMessage
			{
				RequestUri = uri,
				Content = new StringContent(content, Encoding.UTF8, "application/json"),
				Method = HttpMethod.Post,
				Headers =
				{
					{"Accept", "application/json"},
				}
			};
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			
			using (HttpResponseMessage response = await _httpClient.SendAsync(
				request,
				HttpCompletionOption.ResponseHeadersRead))
			{
				using (Stream body = await response.Content.ReadAsStreamAsync())
				using (StreamReader reader = new StreamReader(body))
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();

						// First message is always the one with the current state of Mesos
						if (count == 1)
						{
							// Remove some numbers from the end
							line = line?.Remove(line.LastIndexOf("}", StringComparison.CurrentCultureIgnoreCase) +
												1);

							// Deserialize the object and put it in the application service
							MesosSubscription mesosSubscription =
								JsonConvert.DeserializeObject<MesosSubscription>(line);
							_mesosUpdateHandlerService.ConvertMesosSubscriptions(mesosSubscription);
						}
						else if (count > 1)
						{
							try
							{
								line = line?.Remove(
									line.LastIndexOf("}", StringComparison.CurrentCultureIgnoreCase) +
									1);
								MesosEvent mesosEvent = JsonConvert.DeserializeObject<MesosEvent>(line);
								if (mesosEvent.Type != MesosEventType.HEARTBEAT)
								{
									_mesosUpdateHandlerService.HandleUpdates(mesosEvent);
								}
							}
							catch (JsonException)
							{
								Debug.WriteLine("[MesosCollector] Invalid JSON received. Skipping.");
							}
							catch (ArgumentException e)
							{
								Debug.WriteLine($"[MesosCollector] Invalid event. Exception: {e.Message}.");
							}
							catch (NullReferenceException e)
							{
								Debug.WriteLine($"[MesosCollector] Invalid event. Probably missing a field. Exception: {e.Message}.");
							}
						}
						count++;
					}
			}

			Console.WriteLine("[MesosCollector] Mesos seems to be down. Ending stream.");
			// Since we came to the end of the stream, set the started listening back to false
			StartedListening = false;
		}
	}
}
