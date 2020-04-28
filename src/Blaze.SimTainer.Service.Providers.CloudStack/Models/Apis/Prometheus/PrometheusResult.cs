using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Prometheus
{
	internal class PrometheusResult : IMetric
	{
		[JsonProperty("value")] public string[] Value { get; set; }
		[JsonProperty("metric")] public PrometheusMetric Metric { get; set; }
		public Dictionary<string, double> Values { get; } = new Dictionary<string, double>();
	}
}