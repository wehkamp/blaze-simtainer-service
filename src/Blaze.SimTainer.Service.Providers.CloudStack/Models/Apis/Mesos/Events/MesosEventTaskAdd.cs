using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events
{
	internal class MesosEventTaskAdd
	{
		[JsonProperty("task")] public MesosTask Task { get; set; }
	}
}