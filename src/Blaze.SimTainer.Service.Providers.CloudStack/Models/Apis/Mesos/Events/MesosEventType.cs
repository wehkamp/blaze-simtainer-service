namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos.Events
{
	// Current known event types: https://mesos.apache.org/documentation/latest/operator-http-api/
	internal enum MesosEventType
	{
		TASK_UPDATED,
		TASK_ADDED,
		AGENT_ADDED,
		AGENT_REMOVED,
		HEARTBEAT,
		FRAMEWORK_ADDED,
		FRAMEWORK_UPDATED,
		FRAMEWORK_REMOVED
	}
}