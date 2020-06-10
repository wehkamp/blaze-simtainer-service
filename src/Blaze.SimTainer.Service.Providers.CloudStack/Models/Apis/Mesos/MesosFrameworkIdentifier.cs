using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosFrameworkIdentifier
	{
		[JsonProperty("value")] public string Value { get; set; }
	}
}