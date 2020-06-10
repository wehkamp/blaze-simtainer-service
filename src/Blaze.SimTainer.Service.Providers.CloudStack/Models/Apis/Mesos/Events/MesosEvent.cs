using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events
{
	internal class MesosEvent
	{
		[JsonConverter(typeof(StringEnumConverter))]
		[JsonProperty("type")]
		public MesosEventType Type { get; set; }

		[JsonProperty("task_updated")] public MesosEventTaskUpdate UpdateTask { get; set; }
		[JsonProperty("task_added")] public MesosEventTaskAdd AddTask { get; set; }
		[JsonProperty("framework_added")] public MesosEventFrameworkAdd AddFramework { get; set; }
		[JsonProperty("framework_updated")] public MesosEventFrameworkUpdate UpdatedFramework { get; set; }
		[JsonProperty("framework_removed")] public MesosEventFrameworkRemoved RemovedFramework { get; set; }
	}
}