using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Models;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis
{
	internal class MesosInstance : IInstance
	{
		public string ContainerIdentifier { get; set; }
		public string Identifier { get; }
		public IMetric Metrics { get; set; }
		public InstanceState State { get; set; }
		public string Ip { get; set; }
		public int Port { get; set; }

		public MesosInstance(string containerIdentifier, InstanceState state, string taskIdentifier, string ip = null,
			int port = -1)
		{
			ContainerIdentifier = containerIdentifier;
			State = state;
			Identifier = taskIdentifier;
			Ip = ip;
			Port = port;
		}

		protected bool Equals(MesosInstance other)
		{
			return Identifier == other.Identifier;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MesosInstance)obj);
		}

		public override int GetHashCode()
		{
			return (Identifier != null ? Identifier.GetHashCode() : 0);
		}
	}
}