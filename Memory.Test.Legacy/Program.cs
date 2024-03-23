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
                await jetStream.PublishAsync(subject: "eventsv1.page_loaded", data: data);
                await jetStream.PublishAsync(subject: "eventsv1.mouse_clicked", data: data);
                await jetStream.PublishAsync(subject: "eventsv1.mouse_clicked", data: data);
                await jetStream.PublishAsync(subject: "eventsv1.page_loaded", data: data);
                await jetStream.PublishAsync(subject: "eventsv1.mouse_clicked", data: data);
                await jetStream.PublishAsync(subject: "eventsv1.input_focused", data: data);
            }

            //PrintStreamStateAsync(jetStreamManagement.GetStreamInfo("EVENTS"));
            Console.WriteLine($"V1 Total time taken: {sw.Elapsed.TotalSeconds}");
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