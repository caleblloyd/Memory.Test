using System.Collections.Concurrent;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using System.Diagnostics;

var natsOptions = NatsOpts.Default;
natsOptions = natsOptions with { Url = "localhost" };
await using var nats = new NatsConnection(natsOptions);
var js = new NatsJSContext(nats);
var config = new StreamConfig(name: "EVENTSV2", subjects: new[] { "eventsv2.>" });
config.Storage = StreamConfigStorage.File;
var stream = await js.CreateStreamAsync(config);
await js.PurgeStreamAsync("EVENTSV2", new StreamPurgeRequest());

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
            await js.PublishAsync(subject: "eventsv2.page_loaded", data: data);
            await js.PublishAsync(subject: "eventsv2.mouse_clicked", data: data);
            await js.PublishAsync(subject: "eventsv2.mouse_clicked", data: data);
            await js.PublishAsync(subject: "eventsv2.page_loaded", data: data);
            await js.PublishAsync(subject: "eventsv2.mouse_clicked", data: data);
            await js.PublishAsync(subject: "eventsv2.input_focused", data: data);
            elapsed.Add(sw.Elapsed.TotalSeconds);
            threads.Add(ThreadPool.ThreadCount);
        }
    });
    tasks.Add(task);
}

await Task.WhenAll(tasks);

await stream.RefreshAsync();
var state = stream.Info.State;
Console.WriteLine($"Stream has {state.Messages} messages using {state.Bytes} bytes");
Console.WriteLine($"V2 Avg: {elapsed.Average():F3}, Min: {elapsed.Min():F3}, Max: {elapsed.Max():F3}, Max Threads: {threads.Max()}");
