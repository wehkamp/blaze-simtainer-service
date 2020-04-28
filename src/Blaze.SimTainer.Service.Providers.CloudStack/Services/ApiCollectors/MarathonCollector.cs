using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Marathon;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Services.ApiCollectors
{
	internal class MarathonCollector : IApplicationProvider
	{
		private readonly string _baseUrl;

		private readonly HttpClient _httpClient;

		public IList<MarathonApp> MarathonApps = new List<MarathonApp>();

		private DateTime _lastPolling = DateTime.Now.AddMinutes(-30);
		public MarathonCollector(string baseUrl, HttpClient httpClient)
		{
			_baseUrl = baseUrl;
			_httpClient = httpClient;
			_httpClient.Timeout = TimeSpan.FromSeconds(10);
		}

		public IList<MarathonApp> GetAllApplications()
		{
			HttpResponseMessage response = _httpClient.GetAsync(_baseUrl + "v2/apps").Result;
			
			if (!response.IsSuccessStatusCode) return new List<MarathonApp>();

			HttpContent responseContent = response.Content;

			// We can read it synchronized because we are already working in a different thread due to the service
			string responseString = responseContent.ReadAsStringAsync().Result;

			try
			{
				MarathonModel marathonData = JsonConvert.DeserializeObject<MarathonModel>(responseString);
				return marathonData.Apps;
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception.Message);
			}

			// Return empty list
			return new List<MarathonApp>();
		}

		public void PollData()
		{
			// Poll every 20 minutes
			int result = DateTime.Compare(_lastPolling.AddMinutes(20), DateTime.Now);

			if (result > 0) return;
			MarathonApps = GetAllApplications();
			_lastPolling = DateTime.UtcNow;
		}
	}
}