using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Prometheus;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Services.ApiCollectors
{
	internal class PrometheusCollector : IMetricProvider
	{
		public event EventHandler PrometheusUpdateEvent;

		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;

		private List<PrometheusResult> _cpuMetrics = new List<PrometheusResult>();
		private List<PrometheusResult> _ramMetrics = new List<PrometheusResult>();
		private List<PrometheusResult> _networkMetrics = new List<PrometheusResult>();

		public PrometheusCollector(string baseUrl, HttpClient httpClient)
		{
			_httpClient = httpClient;
			_httpClient.Timeout = TimeSpan.FromSeconds(10);
			_baseUrl = baseUrl;
		}

		public void PollMetrics()
		{
			_cpuMetrics =
				GetMetricList(
					"sum(rate(container_cpu_usage_seconds_total{type=~\"(user|kernel)\"}[2m])) by(role,name) * 100");

			_ramMetrics =
				GetMetricList(
					"sum(avg_over_time(container_memory_usage_bytes[2m])) by (role,name)");

			_networkMetrics =
				GetMetricList(
					"sum(increase(service_http_server{trace_name!=\"*\"}[2m])) by(status, ip, job)");

			// Metrics have been update, send an event
			PrometheusUpdateEvent?.Invoke(null, null);
		}

		/// <summary>
		/// Function to get all the metrics with their instances
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public IMetric GetMetrics(IInstance instance)
		{
			CultureInfo cultureInfo = new CultureInfo("en-US");
			Metric metric = new Metric();
			string identifier = instance.Identifier;
			if (instance is MesosInstance instance1)
			{
				if (instance1.ContainerIdentifier != null)
					identifier = instance1.ContainerIdentifier;

				if (instance1.Ip != null && instance1.Port > 0)
				{
					string instanceIp = $"{instance1.Ip}:{instance1.Port}";
					List<PrometheusResult> instanceMetrics = _networkMetrics
						.Where(x => x.Metric.Ip == instanceIp).ToList();
					if (instanceMetrics.Count > 0)
					{
						double failedNetworkMetrics = instanceMetrics.Where(x => int.Parse(x.Metric.Status) >= 500)
							.Sum(x => Convert.ToDouble(x.Value[1], cultureInfo));
						metric.Values.Add("network_requests_failed",
							Math.Round(failedNetworkMetrics, MidpointRounding.AwayFromZero));
						if (failedNetworkMetrics > 1000)
						{
							Debug.WriteLine("What is happening here? More than 1000 failed requests?");
						}

						double successNetworkMetrics = instanceMetrics.Where(x => int.Parse(x.Metric.Status) < 300)
							.Sum(x => Convert.ToDouble(x.Value[1], cultureInfo));
						metric.Values.Add("network_requests_success",
							Math.Round(successNetworkMetrics, MidpointRounding.AwayFromZero));
					}
				}
			}

			metric.Values.Add("cpu_usage",
				Math.Round(Convert.ToDouble(_cpuMetrics.SingleOrDefault(x =>
						x.Metric.Name == $"mesos-{identifier}")
					?.Value[1], cultureInfo), 1));
			metric.Values.Add("memory_usage",
				Math.Round(ConvertBytesToMegabytes(Convert.ToDouble(
					_ramMetrics.SingleOrDefault(x =>
							x.Metric.Name == $"mesos-{identifier}")
						?.Value[1], cultureInfo)), MidpointRounding.AwayFromZero));

			return metric;
		}

		/// <summary>
		/// Function to retrieve raw metrics for a specific query
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public virtual List<PrometheusResult> GetMetricList(string query)
		{
			string requestUri =
				$"{_baseUrl}api/v1/query?query={query}";

			HttpResponseMessage response = _httpClient.GetAsync(requestUri).Result;

			if (response.IsSuccessStatusCode)
			{
				HttpContent responseContent = response.Content;

				// We can read it synchronized because we are already working in a different thread due to the service
				string responseString = responseContent.ReadAsStringAsync().Result;
				try
				{
					PrometheusModel prometheusModel = JsonConvert.DeserializeObject<PrometheusModel>(responseString);
					return prometheusModel.Data.Result;
				}
				catch (Exception)
				{
					return null;
				}
			}

			return null;
		}

		private static double ConvertBytesToMegabytes(double bytes)
		{
			return bytes / 1024f / 1024f;
		}
	}
}