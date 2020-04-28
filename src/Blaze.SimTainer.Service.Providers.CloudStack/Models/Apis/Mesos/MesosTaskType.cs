namespace Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Mesos
{
	// Types from: https://mesos.apache.org/api/latest/java/org/apache/mesos/Protos.TaskState.html
	internal enum MesosTaskType
	{
		TASK_RUNNING,
		TASK_DROPPED,
		TASK_ERROR,
		TASK_FINISHED,
		TASK_GONE,
		TASK_GONE_BY_OPERATOR,
		TASK_STARTING,
		TASK_KILLED,
		TASK_KILLING,
		TASK_LOST,
		TASK_UNKNOWN,
		TASK_UNREACHABLE,
		TASK_FAILED,
		TASK_STAGING
	}
}
