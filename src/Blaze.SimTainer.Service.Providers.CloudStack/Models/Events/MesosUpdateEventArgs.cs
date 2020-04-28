using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Models;
using System;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Events
{
	internal class MesosUpdateEventArgs : EventArgs
	{
		public ApplicationEventType EventTypeType { get; set; }
		public string Identifier { get; set; }
		public IApplication? Application { get; set; }
		public IInstance? Instance { get; set; }
	}
}