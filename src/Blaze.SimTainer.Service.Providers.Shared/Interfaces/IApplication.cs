using System;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.Shared.Interfaces
{
	public interface IApplication
	{
		public string Name { get; }
		public string Identifier { get; set; }
		public string? Team { get; set; }
		public string Role { get; set; }
		public string Type { get; set; }
		public double Memory { get; set; }
		public double Cpu { get; set; }
		public DateTime Version { get; set; }
		public List<IInstance> Instances { get; set; }
	}
}