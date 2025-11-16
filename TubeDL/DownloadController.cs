using System.Diagnostics;

namespace TubeDL;

public class DownloadController
{
    private volatile bool _isPaused;
    private volatile bool _isCancelled;
    private readonly object _lockObject = new();

    public bool IsPaused => _isPaused;
    public bool IsCancelled => _isCancelled;

    public void Pause()
    {
        lock (_lockObject)
        {
            if (!_isPaused && !_isCancelled)
            {
                _isPaused = true;
                Debug.WriteLine("(controller) Download paused");
            }
        }
    }

    public void Resume()
    {
        lock (_lockObject)
        {
            if (_isPaused)
            {
                _isPaused = false;
                Debug.WriteLine("(controller) Download resumed");

                // Notify semua thread yang waiting
                Monitor.PulseAll(_lockObject);
            }
        }
    }

    public void Cancel()
    {
        lock (_lockObject)
        {
            if (!_isCancelled)
            {
                _isCancelled = true;
                Debug.WriteLine("(controller) Download cancelled");

                // Notify semua thread yang waiting
                Monitor.PulseAll(_lockObject);
            }
        }
    }

    public async Task CheckPauseAndCancel()
    {
        // Cek cancel dulu
        if (_isCancelled)
        {
            throw new OperationCanceledException("Download dibatalkan");
        }

        // Kalau paused, tunggu sampai resume atau cancel
        while (_isPaused && !_isCancelled)
        {
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    if (_isPaused && !_isCancelled)
                    {
                        Debug.WriteLine("(controller) Thread waiting for resume...");
                        Monitor.Wait(_lockObject);
                    }
                }
            });
        }

        // Cek lagi setelah resume
        if (_isCancelled)
        {
            throw new OperationCanceledException("Download dibatalkan");
        }
    }
}