namespace ClassLibrary;
using System.Threading;

public class BgWorker
{
    public delegate void DoWorkEventHandler(object sender, DoWorkEventArgs e);
    public event DoWorkEventHandler DoWorkEvent;

    public delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);
    public event RunWorkerCompletedEventHandler RunWorkerCompletedEvent;

    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);
    public event ProgressChangedEventHandler ProgressChangedEvent;

    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
    public event ErrorEventHandler OnError;

    private Thread workerThread;

    public bool WorkerReportsProgress { get; set; }
    public bool WorkerSupportsCancellation { get; set; }
    public bool CancellationPending { get; private set; }

    public void RunWorkerAsync(object argument)
    {
        workerThread = new Thread(() =>
        {
            try
            {
                var doWorkArgs = new DoWorkEventArgs { Argument = argument };
                DoWorkEvent?.Invoke(this, doWorkArgs);

                if (!doWorkArgs.Cancel)
                {
                    var completedArgs = new RunWorkerCompletedEventArgs();
                    completedArgs.Result = doWorkArgs.Result;
                    RunWorkerCompletedEvent?.Invoke(this, completedArgs);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ErrorEventArgs { Error = ex });
            }
        });

        workerThread.Start();
    }

    public void ReportProgress(double progress)
    {
        var progressArgs = new ProgressChangedEventArgs { ProgressPercentage = progress };
        ProgressChangedEvent?.Invoke(this, progressArgs);
    }

    public void CancelAsync()
    {
        CancellationPending = true;
    }
}

public class DoWorkEventArgs : EventArgs
{
    public bool Cancel { get; set; }
    public object Result { get; set; }
    public object Argument { get; set; }
}

public class RunWorkerCompletedEventArgs : EventArgs
{
    public object Result { get; set; }
    public Exception Error { get; set; }
    public bool Cancelled { get; set; }
}

public class ProgressChangedEventArgs : EventArgs
{
    public double ProgressPercentage { get; set; }

    public string AdditionalInfo { get; set; }
}

public class ErrorEventArgs : EventArgs
{
    public Exception Error { get; set; }
}