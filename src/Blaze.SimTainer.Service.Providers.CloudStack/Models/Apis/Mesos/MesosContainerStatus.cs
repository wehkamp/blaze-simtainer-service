using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosContainerValue
	{
		[JsonProperty("value")] public string Value { get; set; }
	}

	internal class MesosContainerIpAddress
	{
		[JsonProperty("ip_address")] public string IpAddress { get; set; }
	}

	internal class MesosContainerNetworkInfo
	{
		[JsonProperty("ip_addresses")] public List<MesosContainerIpAddress> IpAddressDictionary { get; set; }
		public string IpAddress => IpAddressDictionary.Count == 1 ? IpAddressDictionary[0].IpAddress : string.Empty;
	}

	internal class MesosContainerStatus
	{
		[JsonProperty("container_id")] public Dictionary<string, string> ContainerDictionary { get; set; }
		[JsonProperty("network_infos")] public List<MesosContainerNetworkInfo> NetworkInfo { get; set; }

		public string ContainerId => ContainerDictionary?.SingleOrDefault(x => x.Key == "value").Value;

		public string IpAddress => NetworkInfo.Count == 1 ? NetworkInfo[0].IpAddress : string.Empty;
	}
}