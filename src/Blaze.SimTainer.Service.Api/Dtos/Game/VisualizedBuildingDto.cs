﻿using Blaze.SimTainer.Service.Api.Interfaces;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Api.Dtos.Game
{
	internal class VisualizedBuildingDto : IVisualizedObjectDto
	{
		// Default 15.0
		public string Type { get; } = "building";
		public int Size { get; set; } = 5;
		public Dictionary<string, double> LayerValues { get; set; }
		public string Identifier { get; set; }
	}
}