using Microsoft.Extensions.Logging;
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

var tasks = new List<Task>();
var data = new byte[1024];
for (int i = 0; i < 1000; i++)
{
    var task = Task.Run(async () =>
    {
        while (true)
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 2; i++)
            {
                await js.PublishAsync<object>(subject: "eventsv2.page_loaded", data: data);
                await js.PublishAsync<object>(subject: "eventsv2.mouse_clicked", data: data);
                await js.PublishAsync<object>(subject: "eventsv2.mouse_clicked", data: data);
                await js.PublishAsync<object>(subject: "eventsv2.page_loaded", data: data);
                await js.PublishAsync<object>(subject: "eventsv2.mouse_clicked", data: data);
                await js.PublishAsync<object>(subject: "eventsv2.input_focused", data: data);
            }

            //await PrintStreamStateAsync(stream);
            Console.WriteLine($"V2 Total time taken: {sw.Elapsed.TotalSeconds}");
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





