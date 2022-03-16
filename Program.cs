using SimpleWebServer;

CancellationTokenSource cts = new CancellationTokenSource();
var server = new Server(8080);
var serverTask = server.StartAsync(cts.Token);
Console.WriteLine("Press any key.");
Console.ReadKey();
cts.Cancel();
await serverTask;
Console.ReadKey();