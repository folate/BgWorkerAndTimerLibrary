namespace ClassLibrary;

public class MTimer
{
    private Thread timerThread;
    private CancellationTokenSource cts = new CancellationTokenSource();
    public bool AutoReset { get; set; }
    private bool _enabled;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (_enabled)
            {
                cts = new CancellationTokenSource();
                timerThread = new Thread(new ThreadStart(Run));
                timerThread.IsBackground = true;
                timerThread.Start();
            }
            else
            {
                cts.Cancel();
                timerThread = null;
            }
        }
    }
    public int Interval { get; set; }

    public event Action Elapsed;
    public event Action<Exception> OnError;

    private void Run()
    {
        while (_enabled && !cts.Token.IsCancellationRequested)
        {
            Thread.Sleep(Interval);
            try
            {
                Elapsed?.Invoke();
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
            if (!AutoReset)
            {
                _enabled = false;
            }
        }
    }
    public void Start()
    {
        Enabled = true;
    }
    
    public void Stop()
    {
        Enabled = false;
    }

    public MTimer(int interval)
    {
        Interval = interval;
    }
}