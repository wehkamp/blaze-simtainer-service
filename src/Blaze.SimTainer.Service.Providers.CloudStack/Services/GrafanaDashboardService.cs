using System;
using System.Collections.Generic;
using System.Linq;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using System.Net.Http;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Consul;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Grafana;
using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Services
{
	/// <summary>
	/// This service will handle requests to get a URL of a Grafana dashboard.
	/// </summary>
	internal class GrafanaDashboardService
	{
		private readonly string _grafanaBaseUrl;
		private readonly string _consulBaseUrl;
		private readonly List<string> _requiredGrafanaTags;
		private readonly HttpClient _httpClient;

		public GrafanaDashboardService(string grafanaBaseUrl, string consulBaseUrl, List<string> requiredGrafanaTags,
			HttpClient httpClient)
		{
			_grafanaBaseUrl = grafanaBaseUrl;
			_consulBaseUrl = consulBaseUrl;
			_requiredGrafanaTags = requiredGrafanaTags;
			_httpClient = httpClient;
			_httpClient.Timeout = TimeSpan.FromSeconds(10);
		}

		/// <summary>
		/// Function to get the URL of a dashboard.
		/// </summary>
		/// <param name="application"></param>
		/// <returns></returns>
		public string GetDashboard(IApplication application)
		{
			try
			{
				ConsulKeyValue consulKeyValue = GetConsulIdKeyValue(application);
				if (consulKeyValue == null) return null;

				GrafanaDashboard grafanaDashboard = GetGrafanaDashboard(consulKeyValue.DecodedValue);
				if (grafanaDashboard != null && !string.IsNullOrEmpty(grafanaDashboard.Url))
				{
					return grafanaDashboard.Url.StartsWith("/")
						? $"{_grafanaBaseUrl}{grafanaDashboard.Url.Substring(1)}"
						: $"{_grafanaBaseUrl}{grafanaDashboard.Url}";
				}
			}
			catch (JsonException e)
			{
				Console.WriteLine($"[DashboardService] JSON Error: {e.Message}");
			}

			return null;
		}

		internal virtual ConsulKeyValue GetConsulIdKeyValue(IApplication application)
		{
			HttpResponseMessage response = _httpClient
				.GetAsync(_consulBaseUrl + $"v1/kv/current-service-metadata/{application.Name}/id").Result;

			if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine(
					$"[DashboardService] Invalid response from Consul! Status code: {response.StatusCode}");
				return null;
			}

			HttpContent responseContent = response.Content;

			// We can read it synchronized because we are already working in a different thread due to the service
			string responseString = responseContent.ReadAsStringAsync().Result;
			ConsulKeyValue consulKeyValue = JsonConvert.DeserializeObject<List<ConsulKeyValue>>(responseString)[0];
			return consulKeyValue;
		}

		internal GrafanaDashboard GetGrafanaDashboard(string id)
		{
			HttpResponseMessage response = _httpClient
				.GetAsync(_grafanaBaseUrl + $"api/search?folderIds=0&query={id}%20-&type=dash-db").Result;

			if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine(
					$"[DashboardService] Invalid response from Grafana! Status code: {response.StatusCode}");
				return null;
			}

			HttpContent responseContent = response.Content;

			// We can read it synchronized because we are already working in a different thread due to the service
			string responseString = responseContent.ReadAsStringAsync().Result;

			List<GrafanaDashboard> grafanaDashboards =
				JsonConvert.DeserializeObject<List<GrafanaDashboard>>(responseString);

			if (_requiredGrafanaTags.Count > 0)
			{
				return grafanaDashboards.Where(x => !x.Tags.Except(_requiredGrafanaTags).Any())
					.FirstOrDefault(x => x.Title.IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0);
			}
			return grafanaDashboards.FirstOrDefault(x => x.Title.IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0);
		}
	}
}