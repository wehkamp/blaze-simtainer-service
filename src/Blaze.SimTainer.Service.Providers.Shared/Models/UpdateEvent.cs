using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.Shared.Models
{
	public class UpdateEvent : EventArgs
	{
		public Dictionary<string, Dictionary<string, double>> MetricValues { get; set; }
		public IInstance Instance { get; set; }
		public IApplication Application { get; set; }
		public ApplicationEventType ApplicationEventType { get; set; }
	}
}