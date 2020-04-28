using Blaze.SimTainer.Service.Api.Interfaces;

namespace Blaze.SimTainer.Service.Api.Dtos.Game.Layers
{
	internal class MemoryLayer : IVisualLayer
	{
		public string LayerType { get; } = LayerTypeEnum.memoryLayer.ToString();
		public string Icon { get; } = "images/ram_icon.png";
	}
}