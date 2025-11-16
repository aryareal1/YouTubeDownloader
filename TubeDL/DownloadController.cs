using System.Diagnostics;

namespace TubeDL;

public class DownloadController
{
    private volatile bool _isPaused;
    private volatile bool _isCancelled;
    private readonly object _lockObject = new();

    /// <summary>
    /// Check if the download is currently paused.
    /// </summary>
    public bool IsPaused => _isPaused;
    /// <summary>
    /// Check if the download has been cancelled.
    /// </summary>
    public bool IsCancelled => _isCancelled;

    /// <summary>
    /// Pause the download process.
    /// </summary>
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

    /// <summary>
    /// Resume the download process.
    /// </summary>
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

    /// <summary>
    /// Cancel the download process.
    /// </summary>
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

    /// <summary>
    /// Check if the download is paused or cancelled, and wait if paused.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
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