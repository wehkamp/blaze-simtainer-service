using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Blaze.SimTainer.Service.Providers.Shared.Models;

namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis
{
	internal class MesosInstance : IInstance
	{
		/// <summary>
		/// Full name for task. Is used to kill Mesos instances through Marathon
		/// For example blaze-simtainer-service.instance-2ed4997c-b389-4943-8b58-f19df84cb270._app.1
		/// </summary>
		public string TaskName { get; }

		/// <summary>
		/// Identifier of a Docker container. For example 8496de05-7198-455f-8bff-feddc3b8fbb8. Only exists when container is actually running.
		/// So if the container is not running yet, identifier is the same as the task.
		/// </summary>
		public string ContainerIdentifier { get; set; }

		/// <summary>
		/// Identifier of the task. For example 2ed4997c-b389-4943-8b58-f19df84cb270
		/// </summary>
		public string Identifier { get; }

		/// <summary>
		/// Metrics of an instance.
		/// </summary>
		public IMetric Metrics { get; set; }

		/// <summary>
		/// Current state of an instance.
		/// </summary>
		public InstanceState State { get; set; }

		/// <summary>
		/// IP of an instance. Only exists for some weird reason when a container is staging.
		/// When container is running it gives a different IP. The IP is used for matching metrics.
		/// </summary>
		public string Ip { get; set; }

		/// <summary>
		/// Port of an instance. The port is also used for matching metrics.
		/// </summary>
		public int Port { get; set; }

		public MesosInstance(string containerIdentifier, InstanceState state, string taskIdentifier, string taskName, string ip = null,
			int port = -1)
		{
			ContainerIdentifier = containerIdentifier;
			State = state;
			Identifier = taskIdentifier;
			Ip = ip;
			Port = port;
			TaskName = taskName;
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