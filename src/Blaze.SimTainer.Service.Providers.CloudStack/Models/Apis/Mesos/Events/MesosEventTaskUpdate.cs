using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events
{
	internal class MesosEventTaskUpdate
	{
		[JsonConverter(typeof(StringEnumConverter))]
		[JsonProperty("state")]
		public MesosTaskType State { get; set; }

		[JsonProperty("status")]
		public MesosStatus Status { get; set; }

	}
}