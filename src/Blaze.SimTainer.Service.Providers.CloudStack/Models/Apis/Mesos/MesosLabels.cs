using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosLabel
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}

	internal class MesosLabels
	{
		public List<MesosLabel> Labels { get; set; }
	}
}