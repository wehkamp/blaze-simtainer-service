using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosScalar
	{
		[JsonProperty("value")] public double Value { get; set; }
	}
}
