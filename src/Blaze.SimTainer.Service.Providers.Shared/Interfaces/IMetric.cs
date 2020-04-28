using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.Shared.Interfaces
{
	public interface IMetric
	{
		Dictionary<string, double> Values { get; }
	}
}