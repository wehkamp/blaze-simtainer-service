using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Grafana
{
	internal class GrafanaDashboard
	{
		public string Title { get; set; }

		public string Url { get; set; }

		public List<string> Tags;
	}
}
