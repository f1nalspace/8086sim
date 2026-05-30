using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Final.CPU8086.Mvvm;

public class AsyncCommand<T> : ICommand, INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs IsExecutingChangedArgs = new(nameof(IsExecuting));
    private static readonly PropertyChangedEventArgs IsCancellationRequestedChangedArgs = new(nameof(IsCancellationRequested));

    private readonly Func<T, CancellationToken, Task> _execute;
    private readonly Func<T, bool> _canExecute;
    private readonly Action<Exception> _onException;

    private readonly DelegateCommand _cancelCommand;

    private int _executionCount;
    private CancellationTokenSource _cancellationTokenSource;

    public AsyncCommand(Func<T, CancellationToken, Task> execute, Func<T, bool> canExecute = null, bool allowMultipleExecution = false, Action<Exception> onException = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onException = onException;
        AllowMultipleExecution = allowMultipleExecution;
        _cancelCommand = new DelegateCommand(Cancel, () => IsExecuting && !IsCancellationRequested);
    }

    public event EventHandler CanExecuteChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public bool AllowMultipleExecution { get; }

    public bool IsExecuting => _executionCount > 0;

    public bool IsCancellationRequested => _cancellationTokenSource?.IsCancellationRequested ?? false;

    // Command that cancels the running execution; enabled only while executing.
    public ICommand CancelCommand => _cancelCommand;

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        RaiseOnUI(() =>
        {
            PropertyChanged?.Invoke(this, IsCancellationRequestedChangedArgs);
            _cancelCommand.RaiseCanExecuteChanged();
        });
    }

    public bool CanExecute(object parameter)
    {
        if (!AllowMultipleExecution && IsExecuting)
            return false;
        if (_canExecute is null)
            return true;
        return _canExecute.Invoke(Cast(parameter));
    }

    public async void Execute(object parameter)
    {
        if (!CanExecute(parameter))
            return;
        try
        {
            await ExecuteAsync(Cast(parameter)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is a normal outcome, not an error.
        }
        catch (Exception e)
        {
            // Never let an exception escape an async void (it would crash the process).
            if (_onException != null)
                RaiseOnUI(() => _onException(e));
            else
                RaiseOnUI(() => ExceptionDispatchInfo.Capture(e).Throw());
        }
    }

    public async Task ExecuteAsync(T parameter, CancellationToken externalToken = default)
    {
        CancellationTokenSource cts = externalToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(externalToken)
            : new CancellationTokenSource();
        _cancellationTokenSource = cts;

        Interlocked.Increment(ref _executionCount);
        RaiseStateChanged();
        try
        {
            await _execute(parameter, cts.Token).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref _executionCount);
            if (ReferenceEquals(_cancellationTokenSource, cts))
                _cancellationTokenSource = null;
            cts.Dispose();
            RaiseStateChanged();
        }
    }

    public void RaiseCanExecuteChanged() => RaiseOnUI(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));

    private void RaiseStateChanged() => RaiseOnUI(() =>
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        PropertyChanged?.Invoke(this, IsExecutingChangedArgs);
        _cancelCommand.RaiseCanExecuteChanged();
    });

    private static void RaiseOnUI(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            Dispatcher.UIThread.Post(action);
    }

    private static T Cast(object parameter) => parameter is T value ? value : default;
}

public class AsyncCommand : ICommand, INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs IsExecutingChangedArgs = new(nameof(IsExecuting));
    private static readonly PropertyChangedEventArgs IsCancellationRequestedChangedArgs = new(nameof(IsCancellationRequested));

    private readonly Func<CancellationToken, Task> _execute;
    private readonly Func<bool> _canExecute;
    private readonly Action<Exception> _onException;

    private readonly DelegateCommand _cancelCommand;

    private int _executionCount;
    private CancellationTokenSource _cancellationTokenSource;

    public AsyncCommand(Func<CancellationToken, Task> execute, Func<bool> canExecute = null, bool allowMultipleExecution = false, Action<Exception> onException = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onException = onException;
        AllowMultipleExecution = allowMultipleExecution;
        _cancelCommand = new DelegateCommand(Cancel, () => IsExecuting && !IsCancellationRequested);
    }

    public event EventHandler CanExecuteChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public bool AllowMultipleExecution { get; }

    public bool IsExecuting => _executionCount > 0;

    public bool IsCancellationRequested => _cancellationTokenSource?.IsCancellationRequested ?? false;

    // Command that cancels the running execution; enabled only while executing.
    public ICommand CancelCommand => _cancelCommand;

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        RaiseOnUI(() =>
        {
            PropertyChanged?.Invoke(this, IsCancellationRequestedChangedArgs);
            _cancelCommand.RaiseCanExecuteChanged();
        });
    }

    public bool CanExecute(object parameter)
    {
        if (!AllowMultipleExecution && IsExecuting)
            return false;
        if (_canExecute is null)
            return true;
        return _canExecute.Invoke();
    }

    public async void Execute(object parameter)
    {
        if (!CanExecute(parameter))
            return;
        try
        {
            await ExecuteAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is a normal outcome, not an error.
        }
        catch (Exception e)
        {
            // Never let an exception escape an async void (it would crash the process).
            if (_onException != null)
                RaiseOnUI(() => _onException(e));
            else
                RaiseOnUI(() => ExceptionDispatchInfo.Capture(e).Throw());
        }
    }

    public async Task ExecuteAsync(CancellationToken externalToken = default)
    {
        CancellationTokenSource cts = externalToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(externalToken)
            : new CancellationTokenSource();
        _cancellationTokenSource = cts;

        Interlocked.Increment(ref _executionCount);
        RaiseStateChanged();
        try
        {
            await _execute(cts.Token).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref _executionCount);
            if (ReferenceEquals(_cancellationTokenSource, cts))
                _cancellationTokenSource = null;
            cts.Dispose();
            RaiseStateChanged();
        }
    }

    public void RaiseCanExecuteChanged() => RaiseOnUI(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));

    private void RaiseStateChanged() => RaiseOnUI(() =>
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        PropertyChanged?.Invoke(this, IsExecutingChangedArgs);
        _cancelCommand.RaiseCanExecuteChanged();
    });

    private static void RaiseOnUI(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            Dispatcher.UIThread.Post(action);
    }
}
