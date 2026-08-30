[![](https://img.shields.io/nuget/v/soenneker.utils.executioncontexts.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.executioncontexts/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.executioncontexts/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.executioncontexts/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.executioncontexts.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.executioncontexts/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.executioncontexts/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.executioncontexts/actions/workflows/codeql.yml)

# Soenneker.Utils.ExecutionContexts

Static helpers that run synchronous work inline on context-free threads and offload it when a `SynchronizationContext` is present.

## Installation

```bash
dotnet add package Soenneker.Utils.ExecutionContexts
```

## Usage

```csharp
long bytes = await ExecutionContextUtil.RunInlineOrOffload(
    static path => ScanDirectory(path),
    directoryPath,
    cancellationToken);
```

On a UI or other thread with a non-null `SynchronizationContext`, the function is queued to the thread pool so the context thread is not blocked. On a normal thread-pool or console thread, it executes synchronously before the method returns.

The returned `ValueTask` completes with the action's result or exception. Await each returned value only once; the offloaded path is backed by `IValueTaskSource` rather than a reusable `Task`.

## Cancellation

Cancellation is checked immediately and again before queued work starts. Once the delegate begins, this utility cannot cancel it because no token is passed into the delegate. Put a token in the explicit state when long-running work needs cooperative cancellation:

```csharp
await ExecutionContextUtil.RunInlineOrOffload(
    static state => ProcessFiles(state.Path, state.Token),
    (Path: directoryPath, Token: cancellationToken),
    cancellationToken);
```

## Execution-context behavior

Offloaded work uses `ThreadPool.UnsafeQueueUserWorkItem`, so the caller's `ExecutionContext` is not flowed. `AsyncLocal` values, impersonation state, and other execution-context data should not be assumed available inside the delegate; pass required data explicitly as state.

This utility is intended for synchronous work that may otherwise block a synchronization-context thread. It does not make synchronous I/O asynchronous, limit concurrency, or offload work when the caller has no synchronization context.
