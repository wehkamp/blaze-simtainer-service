using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis
{
	internal class Metric : IMetric
	{
		public Dictionary<string, double> Values { get; } = new Dictionary<string, double>();
	}
}