using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events
{
	internal class MesosEventFrameworkUpdate
	{
		[JsonProperty("framework_info")] public MesosFrameworkInfo FrameworkInfo { get; set; }
	}
}
