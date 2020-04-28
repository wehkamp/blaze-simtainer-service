using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Events
{
	internal class MesosInitializationEventArgs : EventArgs
	{
		public HashSet<IApplication> Applications { get; set; }
	}
}