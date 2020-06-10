using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosState
	{
		[JsonProperty("get_tasks")] public MesosGetTask MesosGetTasks { get; set; }
		[JsonProperty("get_frameworks")] public MesosGetFrameworks MesosGetFrameworks { get; set; }
	}
}
