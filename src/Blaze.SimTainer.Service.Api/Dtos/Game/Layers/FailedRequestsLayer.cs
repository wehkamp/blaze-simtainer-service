using Blaze.SimTainer.Service.Api.Interfaces;

namespace Blaze.SimTainer.Service.Api.Dtos.Game.Layers
{
	internal class FailedRequestsLayer : IVisualLayer
	{
		public string LayerType { get; } = LayerTypeEnum.failedRequestsLayer.ToString();
		public string Icon { get; } = "images/globe_icon.png";
	}
}