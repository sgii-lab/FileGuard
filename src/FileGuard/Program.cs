using FileGuard.Cli;

// Ctrl+C stops 'watch' cleanly instead of killing the process mid-scan.
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

return await App.RunAsync(args, ct: cts.Token);
