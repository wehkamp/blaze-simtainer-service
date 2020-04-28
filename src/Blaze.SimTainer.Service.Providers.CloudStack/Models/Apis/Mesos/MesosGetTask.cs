using Newtonsoft.Json;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosGetTask
	{
		[JsonProperty("tasks")] public List<MesosTask> Tasks { get; set; }
	}
}