namespace superFans.Services;

public class MonitoringService
{
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    public event Action? Tick;

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        _ = RunAsync(_cts.Token);
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        _timer?.Dispose();
        await Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            while (await _timer!.WaitForNextTickAsync(ct))
            {
                Tick?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // Normal stop
        }
    }
}
