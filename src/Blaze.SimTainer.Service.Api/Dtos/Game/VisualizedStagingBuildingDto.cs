using Blaze.SimTainer.Service.Api.Interfaces;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Api.Dtos.Game
{
	internal class VisualizedStagingBuildingDto : IVisualizedObjectDto
	{
		// Default 15.0
		public string Type { get; } = "staging-building";
		public int Size { get; set; } = 5;
		public Dictionary<string, double> LayerValues { get; set; }
		public string Identifier { get; set; }
	}
}