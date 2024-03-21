using NATS.Client.JetStream;
using NATS.Client;
using System.Diagnostics;

Options opts = ConnectionFactory.GetDefaultOptions("localhost");
ConnectionFactory connectionFactory = new ConnectionFactory();
var conn = connectionFactory.CreateConnection(opts);
IJetStream jetStream = conn.CreateJetStreamContext();
IJetStreamManagement jetStreamManagement = conn.CreateJetStreamManagementContext();

jetStreamManagement.AddStream(StreamConfiguration.Builder()
                .WithName("EVENTS")
                .WithStorageType(StorageType.File)
                .WithSubjects("events.>")
                .Build());

var tasks = new List<Task>();
for (int i = 0; i < 1000; i++)
{
    var task = Task.Run(() =>
    {
        while (true)
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 2; i++)
            {
                jetStream.PublishAsync(subject: "events.page_loaded", data: null);
                jetStream.PublishAsync(subject: "events.mouse_clicked", data: null);
                jetStream.PublishAsync(subject: "events.mouse_clicked", data: null);
                jetStream.PublishAsync(subject: "events.page_loaded", data: null);
                jetStream.PublishAsync(subject: "events.mouse_clicked", data: null);
                jetStream.PublishAsync(subject: "events.input_focused", data: null);
            }

            //PrintStreamStateAsync(jetStreamManagement.GetStreamInfo("EVENTS"));
            Console.WriteLine($"Total time taken: {sw.Elapsed.TotalSeconds}");
        }
    });
    tasks.Add(task);
}

await Task.WhenAll(tasks);

void PrintStreamStateAsync(StreamInfo jsStream)
{
    var state = jsStream.State;
    Console.WriteLine($"Stream has {state.Messages} messages using {state.Bytes} bytes");
}