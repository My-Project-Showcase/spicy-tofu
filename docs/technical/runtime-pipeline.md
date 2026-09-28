---
title: Runtime Pipeline
updated: 2026-09-28
sources:
  - ../../Application/Runtime/JsonService/IJsonService.cs
  - ../../Application/Runtime/RunService/IRunService.cs
  - ../../Application/Automation/IAutomationDriver.cs
  - ../../Application/Automation/INavigator.cs
  - ../../Application/Elements/IResolvedElement.cs
  - ../../Infrastructure/Runtime/JsonService/JsonService.cs
  - ../../Infrastructure/Runtime/RunService/RunService.cs
  - ../../Infrastructure/Runtime/TestsLoadedHandler/TestsLoadedHandler.cs
  - ../../Infrastructure/Runtime/EventService/EventService.cs
  - ../../Domain/Events/ITestEvent.cs
  - ../../Domain/Events/EventsRegistry/IEventRegistry.cs
  - ../../Domain/Entities/Execution/TestExecutionStep.cs
  - ../../Domain/Entities/Execution/TestExecutionResult.cs
  - ../../Domain/Entities/Execution/RunResult.cs
  - ../../Application/Elements/IElementRepository.cs
  - ../../Application/Locators/ILocatorResolver.cs
  - ../../Infrastructure/Events/ClickEvent.cs
  - ../../Infrastructure/Events/FillEvent.cs
  - ../../Infrastructure/Events/NavigateEvent.cs
---

# Runtime Pipeline

The framework turns test case JSON files into executed steps in a single linear flow. `JsonService` loads the data, `TestsLoadedHandler` flattens it, and `RunService` orchestrates execution. No application event, message bus, MediatR, or CQRS is involved.

## Flow

`Web/Program.cs` and `Mobile/Program.cs` resolve `IRunService`, call `RunAsync()`, and use the returned `RunResult` to set the process exit code: `0` when no step failed, `1` otherwise. `RunService.RunAsync`:

1. Calls `IAutomationDriver.StartAsync()`, which starts the platform's `default` session.
2. Awaits `IJsonService.LoadJson()`, which returns a `Tuple<bool, List<Test>>`.
3. If the first item is `false` (the configured directory does not exist), logs a warning and returns an empty `RunResult`; the driver is still stopped by the surrounding `finally`.
4. Flattens the loaded tests with `TestsLoadedHandler.Flatten`.
5. For each `TestExecutionStep`, in order: reports it with `ILogger.ActionStarted`, resolves the matching `ITestEvent` through `IEventRegistry.TryGet(step.Step.Type)`, awaits `ITestEvent.ExecuteAsync(step)`, and reports the returned `TestExecutionResult` with `ILogger.ActionCompleted` on success or `ILogger.ActionFailed(step, result.Error)` on failure. A step whose action is not registered, or whose event throws, is recorded as a failure and the run continues with the next step.
6. Logs the totals and returns a `RunResult` with the executed and failed counts.

`StopAsync()` runs in a `finally` block, so the driver is stopped whether the load fails, an event throws, or the run completes.

## Responsibilities

- `JsonService`: reads every `*.json` file under `Projects:RootDirectory`, deserializes each into a `Test` with case-insensitive property matching, and returns the loaded `List<Test>`. It does not flatten, execute, or know about `TestExecutionResult`. A file that fails to parse is reported through `ILogger` and skipped.
- `TestsLoadedHandler`: stateless. Its single method `Flatten` converts a `List<Test>` into `IEnumerable<TestExecutionStep>`, treating a null `Workflows` or `Steps` list as empty:

  ```csharp
  tests.SelectMany(test => (test.Workflows ?? new List<Workflow>())
      .SelectMany(workflow => (workflow.Steps ?? new List<TestSteps>())
          .Select(step => new TestExecutionStep(test, workflow, step))));
  ```

- `TestExecutionStep`: a record in `Domain.Entities.Execution` holding the `Test`, `Workflow`, and `TestSteps` instance produced by the flatten. It describes what should be executed.
- `IEventRegistry` (`EventService`): resolves an action name (`step.Step.Type`) to its `ITestEvent`. `EventService` is built from every registered `ITestEvent`, keyed by the `[Action(...)]` attribute name, case-insensitively. `Get` throws `InvalidOperationException` for an unknown action; `TryGet` returns `false` instead. `RunService` uses `TryGet` so an unknown action fails only that step.
- `ITestEvent`: executes one step and returns a `TestExecutionResult`. Three implementations exist: `ClickEvent` (`[Action("click")]`), `FillEvent` (`[Action("fill")]`), and `NavigateEvent` (`[Action("navigate")]`). `ClickEvent` and `FillEvent` resolve the step's element through `IElementRepository` and `ILocatorResolver` and then call `ClickAsync` or `FillAsync` on the resolved element; `NavigateEvent` drives the platform-neutral `INavigator`. No event holds XPath, application knowledge, or platform types. See [Elements and Locators](./elements-and-locators.md).
- `TestExecutionResult`: describes the outcome of one executed step (`IsSuccess`, `Error`, `Locator`).
- `RunResult`: a record in `Domain.Entities.Execution` with the `Executed` and `Failed` counts. `IsSuccess` is true when `Failed` is zero. `Program.cs` maps it to the process exit code.
- `RunService`: orchestrates the run. It loads, flattens, resolves, executes, and reports. It depends on `IEventRegistry`, not on concrete events, and owns the driver lifecycle. Each step is isolated: an unknown action or a thrown exception becomes a failed step, not a failed run.
- `IAutomationDriver`: `StartAsync` before the load, `StopAsync` in a `finally`. See [Automation Driver Contract](./automation-driver-contract.md).
- `ILogger`: reports each step and its result. See [Logging](./logging.md).

## Execution order

Steps run sequentially, in the order the flatten produces them (test, then workflow, then step). Execution does not stop after the first failing step; every step is resolved and executed, and failures are counted. There is no parallelism today.

## Registration

`AddServices` in `Infrastructure.Extensions.DependencyInjection` registers the logging services and the runtime services, all singletons: `ILogger` (`Logger`), `IPrintStrategy` (`ConsolePrintStrategy`), `IJsonService` (`JsonService`), `TestsLoadedHandler`, `IRunService` (`RunService`), and `IEventRegistry` (`EventService`). It then calls `AddEvents`, which registers each `ITestEvent` implementation (`ClickEvent`, `FillEvent`, `NavigateEvent`) so `EventService` can index them.

## Related pages

- [Domain Layer](./domain-layer.md)
- [Dependency Injection](./dependency-injection.md)
- [Elements and Locators](./elements-and-locators.md)
- [Configuration](./configuration.md)
- [Logging](./logging.md)
