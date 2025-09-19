using Hangfire;

namespace Shared;
public class JobController
{
    private static Object _lock = new();
    public static string Enqueue(Guid guid, int delay, int dataset)
    {
        var datasetId = dataset.ToString();
        lock (_lock)
        {
            var boundQueueIndex = GetBoundQueueIndex(datasetId);
            Console.WriteLine($"JobController.Enqueue: delay={delay} dataset={datasetId} queue={boundQueueIndex}");
            var jobId = BackgroundJob.Enqueue(boundQueueIndex, () => CustomService.Job_Wait(guid, delay, datasetId));
            JobStorage.Current.GetConnection().SetJobParameter(jobId, "boundDatasetId", datasetId);
            Console.WriteLine($"JobController.Enqueue: successful jobId={jobId}");
            return jobId;
        }
    }

    private static string GetBoundQueueIndex(string datasetId)
    {
        var storageConnection = JobStorage.Current.GetConnection();
        var monitor = JobStorage.Current.GetMonitoringApi();
        foreach (var job in monitor.ProcessingJobs(0, int.MaxValue))
        {
            var boundDatasetId = storageConnection.GetJobParameter(job.Key, "boundDatasetId");
            if (boundDatasetId != null)
            {
                if (boundDatasetId == datasetId)
                {
                    return job.Value.Job.Queue;
                }
            }
        }

        var queues = monitor.Queues();
        foreach (var queue in queues)
        {
            foreach (var job in monitor.EnqueuedJobs(queue.Name, 0, int.MaxValue))
            {
                var boundDatasetId = storageConnection.GetJobParameter(job.Key, "boundDatasetId");
                if (boundDatasetId != null)
                {
                    if (boundDatasetId == datasetId)
                    {
                        return job.Value.Job.Queue;
                    }
                }
            }
        }

        Random random = new Random();
        var randomInt = random.Next(1, Consts.ServerCount);
        var newQueue = $"queue{randomInt}";
        return newQueue;
    }
}
