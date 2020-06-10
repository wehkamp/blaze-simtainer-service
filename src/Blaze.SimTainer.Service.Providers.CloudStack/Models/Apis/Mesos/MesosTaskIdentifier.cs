using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	internal class MesosTaskIdentifier
	{
		[JsonProperty("value")] public string Value { get; set; }

		public string TaskIdentifier
		{
			get
			{
				Match m = Regex.Match(Value, "[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?",
					RegexOptions.IgnoreCase);

				if (m.Success)
				{
					return m.Value;
				}

				throw new ArgumentException($"Expected a task identifier. Value was actually: {Value}");
			}
		}

		public string ServiceName => Value.Split(".")[0];

		public override string ToString()
		{
			return Value ?? base.ToString();
		}
	}
}