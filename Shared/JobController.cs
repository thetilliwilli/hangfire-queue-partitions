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
        //var newQueue = $"queue{randomInt}";
        var newQueue = $"queue1";
        return newQueue;
    }

    //private static dynamic GetJobsInfo()
    //{
    //    //var jobs
    //    dynamic result = new { };
    //    var monitoringApi = JobStorage.Current.GetMonitoringApi();
    //    //JobStorage.Current.Moni
    //    var processingJobs = monitoringApi.ProcessingJobs(0, int.MaxValue).Select(x => new
    //    {
    //        Id = x.Key,
    //        Job = x.Value,
    //    }).ToList();

    //    //for ()



    //    var qs = monitoringApi.Queues();
    //    var enqueuedJobs;
    //    foreach (var q in qs)
    //    {
    //        var enqueuedJobs = monitoringApi.EnqueuedJobs(q.Name, 0, int.MaxValue).Select(x => new
    //        {
    //            Id = x.Key,
    //            Job = x.Value
    //        });

    //    }
    //    //Console.WriteLine(JsonSerializer.Serialize(qs));


    //    Console.WriteLine(JsonSerializer.Serialize(result));
    //    return result;
    //}
}
