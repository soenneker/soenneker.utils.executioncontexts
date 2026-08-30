using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Soenneker.Utils.ExecutionContexts;

internal sealed class FuncWorkItem<TState, TResult> : IThreadPoolWorkItem, IValueTaskSource<TResult>
{
    private readonly Func<TState, TResult> _func;
    private readonly TState _state;
    private readonly CancellationToken _cancellationToken;
    private ManualResetValueTaskSourceCore<TResult> _source;

    internal FuncWorkItem(Func<TState, TResult> func, TState state, CancellationToken cancellationToken)
    {
        _func = func;
        _state = state;
        _cancellationToken = cancellationToken;
        _source.RunContinuationsAsynchronously = true;
    }

    internal ValueTask<TResult> Task => new(this, _source.Version);

    void IThreadPoolWorkItem.Execute()
    {
        try
        {
            _cancellationToken.ThrowIfCancellationRequested();
            _source.SetResult(_func(_state));
        }
        catch (Exception exception)
        {
            _source.SetException(exception);
        }
    }

    ValueTaskSourceStatus IValueTaskSource<TResult>.GetStatus(short token) => _source.GetStatus(token);

    TResult IValueTaskSource<TResult>.GetResult(short token) => _source.GetResult(token);

    void IValueTaskSource<TResult>.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
        _source.OnCompleted(continuation, state, token, flags);
}
