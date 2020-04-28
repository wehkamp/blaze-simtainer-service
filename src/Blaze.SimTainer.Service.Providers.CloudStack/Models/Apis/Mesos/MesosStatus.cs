using Blaze.SimTainer.Service.Providers.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosStatus
	{
		[JsonProperty("id")] public string Identifier { get; set; }
		[JsonProperty("name")] public string Name { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[JsonProperty("state")]
		public MesosTaskType State { get; set; }

		[JsonProperty("task_id")] public MesosTaskIdentifier MesosTaskIdentifier { get; set; }
		[JsonProperty("container_status")] public MesosContainerStatus ContainerStatus { get; set; }
		public InstanceState InstanceState
		{
			get
			{
				switch (State)
				{
					case MesosTaskType.TASK_STARTING:
					case MesosTaskType.TASK_STAGING:
						return InstanceState.Staging;
					case MesosTaskType.TASK_RUNNING:
						return InstanceState.Running;
					default:
						return InstanceState.Unknown;
				}
			}
		}
	}
}