---
title: Runtime Pipeline
updated: 2026-09-23
sources:
  - ../../Application/Runtime/JsonService/IJsonService.cs
  - ../../Application/Runtime/RunService/IRunService.cs
  - ../../Infrastructure/Runtime/JsonService/JsonService.cs
  - ../../Infrastructure/Runtime/RunService/RunService.cs
  - ../../Infrastructure/Runtime/TestsLoadedHandler/TestsLoadedHandler.cs
  - ../../Domain/Entities/Execution/TestExecutionStep.cs
---

# Runtime Pipeline

The framework turns test case JSON files into execution-ready steps through a simple application-level event handoff. No message bus, MediatR, or CQRS is involved. The transition is a plain C# event raised by the loader and consumed by the run service.

## Flow

`Web/Program.cs` resolves `IRunService` and calls `RunAsync()`. `RunAsync` subscribes to `IJsonService.TestsLoaded` for the duration of the call, then awaits `LoadJson()`.

`JsonService.LoadJson()` reads every `*.json` file under `Projects:RootDirectory`, deserializes each into a `Test` with case-insensitive property matching, and raises `TestsLoaded` with the loaded `List<Test>`. The event fires only on a successful load; when the directory is missing, `LoadJson` returns the failure tuple and no event fires.

The subscriber, `RunService.OnTestsLoaded`, receives the tests, calls `TestsLoadedHandler.Flatten`, and runs the resulting steps.

`RunAsync` unsubscribes from the event in a `finally` block.

## Responsibilities

- `JsonService`: loads and deserializes JSON files, then signals completion. It does not flatten or execute.
- `TestsLoaded`: an application event, not a domain event. It exists only to signal that loading finished and the `List<Test>` is available.
- `TestsLoadedHandler`: stateless. Its single method `Flatten` converts a `List<Test>` into `IEnumerable<TestExecutionStep>`:

  ```csharp
  tests.SelectMany(test => test.Workflows
      .SelectMany(workflow => workflow.Steps
          .Select(step => new TestExecutionStep(test, workflow, step))));
  ```

- `TestExecutionStep`: a record in `Domain.Entities.Execution` holding the `Test`, `Workflow`, and `TestSteps` instance produced by the flatten.
- `RunService`: subscribes inside `RunAsync`, converts through the handler, and runs the steps. Each flattened step is reported with `ILogger.ActionStarted(step)`. See [Logging](./logging.md).

## Registration

`AddServices` in `Infrastructure.Extensions.DependencyInjection` registers the logging services and the runtime services, all singletons: `ILogger` (`Logger`), `IPrintStrategy` (`ConsolePrintStrategy`), `IJsonService` (`JsonService`), `IRunService` (`RunService`), and `TestsLoadedHandler`.

## Related pages

- [Domain Layer](./domain-layer.md)
- [Dependency Injection](./dependency-injection.md)
- [Configuration](./configuration.md)
- [Logging](./logging.md)