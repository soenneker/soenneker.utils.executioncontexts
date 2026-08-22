using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Soenneker.Utils.ExecutionContexts;

internal sealed class ActionWorkItem<TState> : IThreadPoolWorkItem, IValueTaskSource
{
    private readonly Action<TState> _action;
    private readonly TState _state;
    private ManualResetValueTaskSourceCore<bool> _source;

    internal ActionWorkItem(Action<TState> action, TState state)
    {
        _action = action;
        _state = state;
        _source.RunContinuationsAsynchronously = true;
    }

    internal ValueTask Task => new(this, _source.Version);

    void IThreadPoolWorkItem.Execute()
    {
        try
        {
            _action(_state);
            _source.SetResult(true);
        }
        catch (Exception exception)
        {
            _source.SetException(exception);
        }
    }

    ValueTaskSourceStatus IValueTaskSource.GetStatus(short token) => _source.GetStatus(token);

    void IValueTaskSource.GetResult(short token) => _source.GetResult(token);

    void IValueTaskSource.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
        _source.OnCompleted(continuation, state, token, flags);
}
