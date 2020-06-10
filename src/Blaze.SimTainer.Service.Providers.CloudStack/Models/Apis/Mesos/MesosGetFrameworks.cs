using System.Collections.Generic;
using Newtonsoft.Json;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosGetFrameworks
	{
		[JsonProperty("frameworks")] public List<MesosFramework> Frameworks { get; set; }
	}
}
