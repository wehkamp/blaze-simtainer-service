using System;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Services
{
	/// <summary>
	/// This service will handle task kills.
	/// </summary>
	internal class MesosTaskKillService
	{
		private readonly string _baseUrl;
		private readonly HttpClient _httpClient;

		public MesosTaskKillService(string baseUrl, HttpClient httpClient)
		{
			_baseUrl = baseUrl;
			_httpClient = httpClient;
			_httpClient.Timeout = TimeSpan.FromSeconds(10);
		}

		/// <summary>
		/// Function to kill a task, it sends a delete request to marathon.
		/// </summary>
		/// <param name="application"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public bool KillTask(IApplication application, IInstance instance)
		{
			string url = $"{_baseUrl}v2/apps/{application.Name}/tasks/{application.Name}.{instance.Identifier}";
			Task<HttpResponseMessage> request = _httpClient.DeleteAsync(url);
			HttpResponseMessage result = request.Result;
			return result.IsSuccessStatusCode;
		}
	}
}