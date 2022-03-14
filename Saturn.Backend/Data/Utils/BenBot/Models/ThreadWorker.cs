using System;
using System.Threading;
using System.Threading.Tasks;
using CUE4Parse.Utils;
using Saturn.Backend.Data.Enums;
using Serilog;

namespace Saturn.Backend.Data.Utils.BenBot.Models;

public class ThreadWorker : ViewModel
{
    EStatusKind _status;

    private bool _statusChangeAttempted;
    public bool StatusChangeAttempted
    {
        get => _statusChangeAttempted;
        private set => SetProperty(ref _statusChangeAttempted, value);
    }

    private bool _operationCancelled;

    public bool OperationCancelled
    {
        get => _operationCancelled;
        private set => SetProperty(ref _operationCancelled, value);
    }

    private CancellationTokenSource _currentCancellationTokenSource;

    public CancellationTokenSource CurrentCancellationTokenSource
    {
        get => _currentCancellationTokenSource;
        set
        {
            if (_currentCancellationTokenSource == value) return;
            SetProperty(ref _currentCancellationTokenSource, value);
            RaisePropertyChanged("CanBeCanceled");
        }
    }

    public bool CanBeCanceled => CurrentCancellationTokenSource != null;

    private readonly AsyncQueue<Action<CancellationToken>> _jobs;

    public ThreadWorker()
    {
        _status = EStatusKind.Loading;
        _jobs = new AsyncQueue<Action<CancellationToken>>();
        _status = EStatusKind.Ready;
    }

    public async Task Begin(Action<CancellationToken> action)
    {
        if (_status != EStatusKind.Ready)
        {
            SignalOperationInProgress();
            return;
        }

        CurrentCancellationTokenSource ??= new CancellationTokenSource();
        _jobs.Enqueue(action);
        await ProcessQueues();
    }

    public void Cancel()
    {
        if (!CanBeCanceled)
        {
            SignalOperationInProgress();
            return;
        }

        CurrentCancellationTokenSource.Cancel();
    }

    private async Task ProcessQueues()
    {
        if (_jobs.Count > 0)
        {
            _status = EStatusKind.Loading;
            await foreach (var job in _jobs)
            {
                try
                {
                    // will end in "catch" if canceled
                    await Task.Run(() => job(CurrentCancellationTokenSource.Token));
                }
                catch (OperationCanceledException)
                {
                    _status = EStatusKind.Stopped;
                    CurrentCancellationTokenSource = null; // kill token
                    OperationCancelled = true;
                    OperationCancelled = false;
                    return;
                }
                catch (Exception e)
                {
                    _status = EStatusKind.Failed;
                    CurrentCancellationTokenSource = null; // kill token

                    Log.Error("{Exception}", e);

                    Logger.Log(e.Message, LogLevel.Error);
                    Logger.Log("      " + e.StackTrace.SubstringBefore('\n').Trim(), LogLevel.Error);
                    return;
                }
            }

            _status = EStatusKind.Completed;
            CurrentCancellationTokenSource = null; // kill token
        }
    }

    public void SignalOperationInProgress()
    {
        StatusChangeAttempted = true;
        StatusChangeAttempted = false;
    }
}