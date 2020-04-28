using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosSubscription
	{
		[JsonProperty("type")] public string Type { get; set; }
		[JsonProperty("subscribed")] public MesosSubscribed MesosSubscribed { get; set; }
	}
}
