using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events
{
	internal class MesosEventFrameworkAdd
	{
		[JsonProperty("framework_info")] public MesosFrameworkInfo FrameworkInfo { get; set; }
	}
}
