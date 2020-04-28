using System;

namespace Blaze.SimTainer.Service.Providers.Shared.Interfaces
{
	public interface IMetricProvider
	{
		public event EventHandler PrometheusUpdateEvent;
		IMetric GetMetrics(IInstance instance);
		void PollMetrics();
	}
}