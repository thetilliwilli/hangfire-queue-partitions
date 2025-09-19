using Hangfire;
using Shared;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var appIndex = Assembly.GetExecutingAssembly().GetName().Name.Last().ToString();
var appName = $"srv{appIndex}";
Console.WriteLine($"App started: {appName}");
builder.Services.AddHangfire(appName);


var app = builder.Build();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DisplayStorageConnectionString = true
});

app.MapGet("/job/{delay:int}/dataset/{dataset:int}", (int delay, int dataset) =>
{
    var guid = Guid.NewGuid();
    JobController.Enqueue(guid, delay, dataset);
    return "ok";
});

app.Run();