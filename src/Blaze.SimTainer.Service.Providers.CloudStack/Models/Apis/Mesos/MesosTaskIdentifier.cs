using Newtonsoft.Json;
using System;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosTaskIdentifier
	{
		[JsonProperty("value")] public string Value { get; set; }

		public string TaskIdentifier
		{
			get
			{
				string[] val = Value.Split(".");
				return val.Length == 2
					? val[1]
					: throw new ArgumentException($"Expected a task identifier. Value was actually: {Value}");
			}
		}

		public string ServiceName => Value.Split(".")[0];

		public override string ToString()
		{
			return Value ?? base.ToString();
		}
	}
}