using Newtonsoft.Json;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosDockerPortMapping
	{
		[JsonProperty("host_port")] public int HostPort { get; set; }
	}

	internal class MesosDockerContainer
	{
		[JsonProperty("port_mappings")] public List<MesosDockerPortMapping> PortMappingDictionary { get; set; }

		public int HostPort => PortMappingDictionary?.Count == 1 ? PortMappingDictionary[0].HostPort : -1;
	}

	internal class MesosContainer
	{
		[JsonProperty("docker")] public MesosDockerContainer DockerContainer { get; set; }
	}
}