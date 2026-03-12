using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Invocations.Actions;
using Soenneker.Invocations.Funcs;

namespace Soenneker.Utils.ExecutionContexts;

/// <summary>
/// Utilities for executing work inline or offloading to the thread pool based on the current synchronization context.
/// </summary>
public sealed class ExecutionContextUtil
{
    /// <summary>
    /// Determines whether the current thread is associated with a synchronization context.
    /// </summary>
    /// <remarks>A synchronization context is typically present in UI threads or environments that require
    /// marshaling of work to a specific thread, such as Windows Forms or WPF applications. In thread pool or background
    /// threads, this method usually returns <see langword="false"/>.</remarks>
    /// <returns><see langword="true"/> if the current thread has a synchronization context; otherwise, <see langword="false"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool OnSynchronizationContext() => SynchronizationContext.Current is not null;

    /// <summary>
    /// Executes the specified action either inline or offloads it to the thread pool, depending on the current
    /// synchronization context.
    /// </summary>
    /// <remarks>If called from a synchronization context, the action is scheduled to run asynchronously on
    /// the thread pool. Otherwise, the action is executed synchronously on the current thread. If the cancellation
    /// token is already canceled, the returned ValueTask is in the canceled state.</remarks>
    /// <param name="action">The action to execute. Cannot be null.</param>
    /// <param name="state">An object containing data to be passed to the action. May be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation before it starts.</param>
    /// <returns>A ValueTask that represents the execution of the action. The task is completed when the action has finished
    /// executing, or is canceled if the cancellation token is already canceled.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask RunInlineOrOffload(Action<object?> action, object? state, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled(cancellationToken);

        if (OnSynchronizationContext())
        {
            Task task = Task.Factory.StartNew(static s => ((ActionInvocation)s!).Invoke(), new ActionInvocation(action, state), cancellationToken,
                TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);

            return new ValueTask(task);
        }

        action(state);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Executes the specified function either inline or offloads it to the thread pool, depending on the current
    /// synchronization context and cancellation state.
    /// </summary>
    /// <remarks>If called from a synchronization context, the function is executed asynchronously on the
    /// thread pool to avoid blocking the context. Otherwise, the function is executed inline on the current thread. If
    /// the cancellation token is already canceled, the method returns a canceled ValueTask immediately.</remarks>
    /// <typeparam name="T">The type of the result produced by the function.</typeparam>
    /// <param name="func">The function to execute. The function receives the specified state object as its parameter. Cannot be null.</param>
    /// <param name="state">An object containing data to be used by the function, or null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation before execution begins.</param>
    /// <returns>A ValueTask representing the result of the function execution. If the operation is canceled before execution,
    /// the ValueTask is in a canceled state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> RunInlineOrOffload<T>(Func<object?, T> func, object? state, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled<T>(cancellationToken);

        if (OnSynchronizationContext())
        {
            Task<T> task = Task.Factory.StartNew(static s => ((FuncInvocation<T>)s!).Invoke(), new FuncInvocation<T>(func, state), cancellationToken,
                TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);

            return new ValueTask<T>(task);
        }

        return new ValueTask<T>(func(state));
    }
}