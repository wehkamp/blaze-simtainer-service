using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis
{
	internal class MesosApplication : IApplication
	{
		public string Name { get; set; }
		public string Identifier { get; set; }

		public List<IInstance> Instances { get; set; } = new List<IInstance>();

		public string Team { get; set; }
		public string Role { get; set; }
		public string Type { get; set; }
		public double Memory { get; set; }
		public double Cpu { get; set; }
		public DateTime Version { get; set; }
	}
}