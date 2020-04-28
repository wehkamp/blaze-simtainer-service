using Blaze.SimTainer.Service.Providers.Shared.Models;

namespace Blaze.SimTainer.Service.Providers.Shared.Interfaces
{
	public interface IInstance
	{
		public string Identifier { get; }
		public IMetric Metrics { get; set; }
		public InstanceState State { get; set; }
	}
}