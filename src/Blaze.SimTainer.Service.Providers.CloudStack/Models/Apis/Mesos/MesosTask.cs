using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosTask
	{
		[JsonProperty("task_id")] public MesosTaskIdentifier Identifier { get; set; }
		[JsonProperty("framework_id")] public MesosFrameworkIdentifier FrameworkIdentifier { get; set; }
		[JsonProperty("name")] public string Name { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[JsonProperty("state")]
		public MesosTaskType State { get; set; }
		[JsonProperty("resources")] public List<MesosResource> Resources { get; set; }
		[JsonProperty("statuses")] public List<MesosStatus> Statuses { get; set; }
		[JsonProperty("container")] public MesosContainer Container { get; set; }
		[JsonProperty("labels")] public MesosLabels? Labels { get; set; }
	}
}