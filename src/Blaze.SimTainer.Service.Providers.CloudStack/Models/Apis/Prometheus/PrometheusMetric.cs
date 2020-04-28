using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Prometheus
{
	internal class PrometheusMetric
	{
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("status")] public string Status { get; set; }
		[JsonProperty("trace_name")] public string TraceName { get; set; }
		[JsonProperty("id")] public string Identifier { get; set; }
		[JsonProperty("instance")] public string Instance { get; set; }
		[JsonProperty("role")] public string Role { get; set; }
		[JsonProperty("job")] public string Job { get; set; }
		[JsonProperty("type")] public string? Type { get; set; }
		[JsonProperty("ip")] public string? Ip { get; set; }
	}
}