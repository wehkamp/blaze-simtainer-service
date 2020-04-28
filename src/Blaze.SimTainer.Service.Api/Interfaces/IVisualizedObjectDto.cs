using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Api.Interfaces
{
	public interface IVisualizedObjectDto
	{
		public string Type { get; }
		public int Size { get; set; }
		public Dictionary<string, double> LayerValues { get; set; }
		public string Identifier { get; set; }
	}
}