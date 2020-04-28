using Blaze.SimTainer.Service.Api.Dtos.Game.Layers;
using Blaze.SimTainer.Service.Api.Interfaces;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Api.Dtos.Game
{
	public class NeighbourhoodDto
	{
		public string Name { get; set; }
		public List<IVisualizedObjectDto> VisualizedObjects { get; set; } = new List<IVisualizedObjectDto>();
		public List<LayerValueDto> LayerValues { get; set; }
		public int DaysOld { get; set; }
		public string Team { get; set; }
	}
}