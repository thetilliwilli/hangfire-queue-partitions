using Hangfire;

namespace Shared;
public static class CustomService
{
    public static Dictionary<string, int> DatasetVersion = new();


    [AutomaticRetry(Attempts = 3)]
    public static void Job_Wait(Guid guid, int delay, string datasetId)
    {
        Console.WriteLine($"\tStart.Job_Wait: dataset = {datasetId}");
        if (!DatasetVersion.ContainsKey(datasetId))
            DatasetVersion[datasetId] = 0;

        DatasetVersion[datasetId] = DatasetVersion[datasetId] + 1;
        var startDatasetVersion = DatasetVersion[datasetId];

        Console.WriteLine($"\t\tDatasetVersion={DatasetVersion[datasetId]}");
        Thread.Sleep(delay * 1000);
        Console.WriteLine($"\t\tDatasetVersion={DatasetVersion[datasetId]}");

        if (startDatasetVersion != DatasetVersion[datasetId])
            throw new Exception($"Dataset Version mismatch: startDatasetVersion={startDatasetVersion} currentDatasetVersion={DatasetVersion[datasetId]}");

        Console.WriteLine($"\tEnd.Job_Wait: dataset = {datasetId}");
    }
}
