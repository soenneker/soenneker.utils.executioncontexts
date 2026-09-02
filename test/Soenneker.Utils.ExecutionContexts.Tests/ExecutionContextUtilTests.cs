using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Tests.Unit;

namespace Soenneker.Utils.ExecutionContexts.Tests;

public sealed class ExecutionContextUtilTests : UnitTest
{
    [Test]
    public async Task RunInlineOrOffload_WithoutSynchronizationContext_RunsInline(CancellationToken cancellationToken)
    {
        SynchronizationContext? originalContext = SynchronizationContext.Current;

        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            int callingThread = Environment.CurrentManagedThreadId;

            ValueTask<int> task = ExecutionContextUtil.RunInlineOrOffload(static state => Environment.CurrentManagedThreadId + state, 1, cancellationToken: cancellationToken);

            await Assert.That(task.IsCompletedSuccessfully).IsTrue();
            await Assert.That(await task).IsEqualTo(callingThread + 1);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }
    }

    [Test]
    public async Task RunInlineOrOffload_WithSynchronizationContext_OffloadsAction(CancellationToken cancellationToken)
    {
        SynchronizationContext? originalContext = SynchronizationContext.Current;
        var holder = new StrongBox<SynchronizationContext?>();
        ValueTask task;

        try
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            task = ExecutionContextUtil.RunInlineOrOffload(static state => { state.Value = SynchronizationContext.Current; }, holder, cancellationToken: cancellationToken);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }

        await task;
        await Assert.That(holder.Value).IsNull();
    }

    [Test]
    public async Task RunInlineOrOffload_WithSynchronizationContext_ReturnsResult(CancellationToken cancellationToken)
    {
        SynchronizationContext? originalContext = SynchronizationContext.Current;
        ValueTask<int> task;

        try
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            task = ExecutionContextUtil.RunInlineOrOffload(static state => state * 2, 21, cancellationToken: cancellationToken);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }

        await Assert.That(await task).IsEqualTo(42);
    }

    [Test]
    public async Task RunInlineOrOffload_WithSynchronizationContext_PropagatesException(CancellationToken cancellationToken)
    {
        SynchronizationContext? originalContext = SynchronizationContext.Current;
        ValueTask task;

        try
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            task = ExecutionContextUtil.RunInlineOrOffload((Action<int>) (static _ => throw new InvalidOperationException("Expected")), 0, cancellationToken: cancellationToken);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }

        await Assert.ThrowsAsync<InvalidOperationException>(() => task.AsTask());
    }

    [Test]
    public async Task RunInlineOrOffload_WithCanceledToken_DoesNotExecute()
    {
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();
        var holder = new StrongBox<bool>();

        ValueTask task = ExecutionContextUtil.RunInlineOrOffload(static state => { state.Value = true; }, holder, cancellationSource.Token);

        await Assert.ThrowsAsync<TaskCanceledException>(() => task.AsTask());
        await Assert.That(holder.Value).IsFalse();
    }

}
