using Blaze.SimTainer.Service.Api.Interfaces;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Api.Dtos.Game
{
	internal class VisualizedVehicleDto : IVisualizedObjectDto
	{
		public string Type { get; } = "vehicle";
		public Dictionary<string, double> LayerValues { get; set; }
		public string Identifier { get; set; }
		public int Size { get; set; } = 15;
	}
}