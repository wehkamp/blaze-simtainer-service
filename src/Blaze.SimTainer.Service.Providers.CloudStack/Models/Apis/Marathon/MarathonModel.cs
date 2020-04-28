using Newtonsoft.Json;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Marathon
{
	internal class MarathonModel
	{
		[JsonProperty("apps")] public IList<MarathonApp> Apps { get; set; }

	}
}
