using Newtonsoft.Json;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Prometheus
{
	internal class PrometheusData
	{
		[JsonProperty("resultType")] public string ResultType { get; set; }
		[JsonProperty("result")] public List<PrometheusResult> Result { get; set; }
	}
}