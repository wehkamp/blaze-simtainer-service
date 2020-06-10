using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosFramework
	{
		[JsonProperty("framework_info")] public MesosFrameworkInfo FrameworkInfo { get; set; }
	}
}
