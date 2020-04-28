using Blaze.SimTainer.Service.Api.Interfaces;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Api.Dtos.Game
{
	public class GameDto
	{
		public HashSet<NeighbourhoodDto> Neighbourhoods { get; set; }
		public IVisualLayer[] Layers { get; set; }
		public HashSet<string> Teams { get; set; }
	}
}