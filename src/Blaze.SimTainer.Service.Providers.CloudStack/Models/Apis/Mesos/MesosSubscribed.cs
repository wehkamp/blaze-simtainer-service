using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosSubscribed
	{
		[JsonProperty("get_state")] public MesosState State { get; set; }
		[JsonProperty("heartbeat_interval_seconds")] public double HeartbeatInterval { get; set; }
	}
}
