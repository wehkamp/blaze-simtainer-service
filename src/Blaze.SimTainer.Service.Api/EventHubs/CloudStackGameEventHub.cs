using Blaze.SimTainer.Service.Api.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Blaze.SimTainer.Service.Api.EventHubs
{
	/// <summary>
	/// SignalR event hub for the cloud stack game entity
	/// This hub is used in the <see cref="Services.ApiCollectorService"/>
	/// </summary>
	internal class CloudStackGameEventHub : Hub<IGameEventHub>
	{
		public override Task OnDisconnectedAsync(Exception exception)
		{
			Debug.WriteLine($"Client disconnected {Context.ConnectionId}");
			return base.OnDisconnectedAsync(exception);
		}

		public override Task OnConnectedAsync()
		{
			Debug.WriteLine($"Client connected {Context.ConnectionId}");
			return base.OnConnectedAsync();
		}
	}
}