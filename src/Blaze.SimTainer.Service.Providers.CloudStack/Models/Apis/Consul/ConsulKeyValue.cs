using System;
using System.Collections.Generic;
using System.Text;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Consul
{
	internal class ConsulKeyValue
	{
		public string DecodedValue => Encoding.UTF8.GetString(Convert.FromBase64String(Value));

		public string Value { get; set; }
	}
}