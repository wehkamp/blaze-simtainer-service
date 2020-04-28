using Newtonsoft.Json;
using System;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Marathon
{
	internal class MarathonApp
	{
		[JsonProperty("id")] public string Identifier { get; set; }
		public string Name => Identifier.Replace("/", "");
		[JsonProperty("version")] public DateTime Version { get; set; }
	}
}