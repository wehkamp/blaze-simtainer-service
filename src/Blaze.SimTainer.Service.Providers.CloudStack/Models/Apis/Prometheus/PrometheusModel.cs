using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Prometheus
{
	internal class PrometheusModel
	{
		[JsonProperty("status")] public string Status { get; set; }
		[JsonProperty("data")] public PrometheusData Data { get; set; }
	}
}