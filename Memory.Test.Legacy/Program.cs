using System.Collections.Concurrent;
using NATS.Client.JetStream;
using NATS.Client;
using System.Diagnostics;

Options opts = ConnectionFactory.GetDefaultOptions("localhost");
ConnectionFactory connectionFactory = new ConnectionFactory();
var conn = connectionFactory.CreateConnection(opts);
IJetStream jetStream = conn.CreateJetStreamContext();
IJetStreamManagement jetStreamManagement = conn.CreateJetStreamManagementContext();

jetStreamManagement.AddStream(StreamConfiguration.Builder()
                .WithName("EVENTSV1")
                .WithStorageType(StorageType.File)
                .WithSubjects("eventsv1.>")
                .Build());
jetStreamManagement.PurgeStream("EVENTSV1");

var tasks = new List<Task>();
var data = new byte[1024];
var elapsed = new ConcurrentBag<double>();
var threads = new ConcurrentBag<int>();

for (var i = 0; i < 1000; i++)
{
    var task = Task.Run(async () =>
    {
        var sw = new Stopwatch();
        for (var j = 0; j < 100; j++)
        {
            sw.Restart();
            await jetStream.PublishAsync(subject: "eventsv1.page_loaded", data: data);
            await jetStream.PublishAsync(subject: "eventsv1.mouse_clicked", data: data);
            await jetStream.PublishAsync(subject: "eventsv1.mouse_clicked", data: data);
            await jetStream.PublishAsync(subject: "eventsv1.page_loaded", data: data);
            await jetStream.PublishAsync(subject: "eventsv1.mouse_clicked", data: data);
            await jetStream.PublishAsync(subject: "eventsv1.input_focused", data: data);
            elapsed.Add(sw.Elapsed.TotalSeconds);
            threads.Add(ThreadPool.ThreadCount);
        }
    });
    tasks.Add(task);
}

await Task.WhenAll(tasks);

var stream = jetStreamManagement.GetStreamInfo("EVENTSV1");
var state = stream.State;
Console.WriteLine($"Stream has {state.Messages} messages using {state.Bytes} bytes");
Console.WriteLine($"V1 Avg: {elapsed.Average():F3}, Min: {elapsed.Min():F3}, Max: {elapsed.Max():F3}, Max Threads: {threads.Max()}");
