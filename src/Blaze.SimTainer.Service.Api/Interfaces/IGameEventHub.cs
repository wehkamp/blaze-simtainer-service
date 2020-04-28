using System.Threading.Tasks;

namespace Blaze.SimTainer.Service.Api.Interfaces
{
	public interface IGameEventHub
	{
		Task SendUpdateToClients(string json);
	}
}