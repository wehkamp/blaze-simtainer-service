using Blaze.SimTainer.Service.Api.Interfaces;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Api.Dtos.Game
{
	internal class UpdateEventDto
	{
		public string NeighbourhoodName { get; set; }
		public NeighbourhoodDto AddedNeighbourhood { get; set; }
		public string RemovedNeighbourhood { get; set; }
		public IVisualizedObjectDto AddedVisualizedObject { get; set; }
		public string RemovedVisualizedObject { get; set; }
		public Dictionary<string, Dictionary<string, double>> UpdatedLayerValues { get; set; }
		public NeighbourhoodDto UpdatedNeighbourhood { get; set; }
		public List<IVisualizedObjectDto> UpdatedVisualizedObjects { get; set; }
	}
}