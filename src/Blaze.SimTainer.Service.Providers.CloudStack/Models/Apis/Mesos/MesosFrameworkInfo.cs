using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosFrameworkInfo
	{
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("id")] public MesosFrameworkIdentifier Identifier { get; set; }
	}
}
