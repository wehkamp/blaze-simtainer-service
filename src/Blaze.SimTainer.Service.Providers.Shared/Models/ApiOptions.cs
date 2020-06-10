using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.Shared.Models
{
	public class ApiOptions
	{
		public string PrometheusEndpoint { get; set; }
		public string MesosEndpoint { get; set; }
		public string MesosFramework { get; set; }
		public string MarathonEndpoint { get; set; }
		public string ConsulEndpoint { get; set; }
		public string GrafanaEndpoint { get; set; }
		public List<string> RequiredGrafanaTags { get; set; }
		public int PollingSeconds { get; set; }
	}
}