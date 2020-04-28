namespace Blaze.SimTainer.Service.Api.Interfaces
{
	public enum LayerTypeEnum
	{
		cpuLayer,
		memoryLayer,
		failedRequestsLayer,
	}

	public interface IVisualLayer
	{
		public string Icon { get; }
		public string LayerType { get; }
	}
}