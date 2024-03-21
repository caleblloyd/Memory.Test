using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using System.Diagnostics;

var natsOptions = NatsOpts.Default;
natsOptions = natsOptions with { Url = "localhost" };
await using var nats = new NatsConnection(natsOptions);
var js = new NatsJSContext(nats);
var config = new StreamConfig(name: "EVENTS", subjects: new[] { "events.>" });
config.Storage = StreamConfigStorage.File;
var stream = await js.CreateStreamAsync(config);

var tasks = new List<Task>();
for (int i = 0; i < 1000; i++)
{
    var task = Task.Run(async () =>
    {
        while (true)
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 2; i++)
            {
                await js.PublishAsync<object>(subject: "events.page_loaded", data: null);
                await js.PublishAsync<object>(subject: "events.mouse_clicked", data: null);
                await js.PublishAsync<object>(subject: "events.mouse_clicked", data: null);
                await js.PublishAsync<object>(subject: "events.page_loaded", data: null);
                await js.PublishAsync<object>(subject: "events.mouse_clicked", data: null);
                await js.PublishAsync<object>(subject: "events.input_focused", data: null);
            }

            //await PrintStreamStateAsync(stream);
            Console.WriteLine($"Total time taken: {sw.Elapsed.TotalSeconds}");
        }
    });
    tasks.Add(task);
}

await Task.WhenAll(tasks);

async Task PrintStreamStateAsync(INatsJSStream jsStream)
{
    await jsStream.RefreshAsync();
    var state = jsStream.Info.State;
    Console.WriteLine($"Stream has {state.Messages} messages using {state.Bytes} bytes");
}





