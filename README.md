[![](https://img.shields.io/nuget/v/soenneker.utils.executioncontexts.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.executioncontexts/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.executioncontexts/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.executioncontexts/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.utils.executioncontexts.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.utils.executioncontexts/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.executioncontexts/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.executioncontexts/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.ExecutionContexts
Utilities for executing work inline or offloading to the thread pool based on the current synchronization context.

## Installation

```bash
dotnet add package Soenneker.Utils.ExecutionContexts
```

## Quick start

```csharp
using Soenneker.Utils.ExecutionContexts;
```

Create a `ExecutionContextUtil` instance, then use the operation you need below.

## Common operations

- `OnSynchronizationContext()` - Determines whether the current thread is associated with a synchronization context. A synchronization context is typically present in UI threads or environments that require marshaling of work to a specific thread, such as Windows Forms or WPF applications.
- `RunInlineOrOffload()` - Executes the specified action either inline or offloads it to the thread pool, depending on the current synchronization context. If called from a synchronization context, the action is scheduled to run asynchronously on the thread pool.
