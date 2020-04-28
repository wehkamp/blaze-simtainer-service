using Blaze.SimTainer.Service.Api.Interfaces;

namespace Blaze.SimTainer.Service.Api.Dtos.Game.Layers
{
	internal class CpuLayer : IVisualLayer
	{
		public string LayerType { get; } = LayerTypeEnum.cpuLayer.ToString();
		public string Icon { get; set; } = "images/cpu_icon.png";
	}
}