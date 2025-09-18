using Hangfire;

namespace Shared;
public class JobController
{
    public static string Enqueue(Guid guid, int delay, int dataset)
    {
        var queue = "queue" + dataset;
        Console.WriteLine($"JobController.Enqueue: delay={delay} dataset={dataset} queue={queue}");
        var jobId = BackgroundJob.Enqueue(queue , () => HangfireService.Job_Wait(guid, delay, dataset));
        Console.WriteLine($"JobController.Enqueue: successful jobId={jobId}");
        return jobId;
    }
}
