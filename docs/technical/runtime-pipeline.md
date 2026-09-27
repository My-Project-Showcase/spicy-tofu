---
title: Runtime Pipeline
updated: 2026-09-27
sources:
  - ../../Application/Runtime/JsonService/IJsonService.cs
  - ../../Application/Runtime/RunService/IRunService.cs
  - ../../Application/Automation/IAutomationDriver.cs
  - ../../Infrastructure/Runtime/JsonService/JsonService.cs
  - ../../Infrastructure/Runtime/RunService/RunService.cs
  - ../../Infrastructure/Runtime/TestsLoadedHandler/TestsLoadedHandler.cs
  - ../../Infrastructure/Runtime/EventService/EventService.cs
  - ../../Domain/Events/ITestEvent.cs
  - ../../Domain/Events/EventsRegistry/IEventRegistry.cs
  - ../../Domain/Entities/Execution/TestExecutionStep.cs
  - ../../Domain/Entities/Execution/TestExecutionResult.cs
  - ../../Application/Elements/IElementRepository.cs
  - ../../Application/Locators/ILocatorResolver.cs
  - ../../Infrastructure/Events/ClickEvent.cs
---

# Runtime Pipeline

The framework turns test case JSON files into executed steps in a single linear flow. `JsonService` loads the data, `TestsLoadedHandler` flattens it, and `RunService` orchestrates execution. No application event, message bus, MediatR, or CQRS is involved.

## Flow

`Web/Program.cs` and `Mobile/Program.cs` resolve `IRunService` and call `RunAsync()`. `RunService.RunAsync`:

1. Calls `IAutomationDriver.StartAsync()`, which starts the platform's `default` session.
2. Awaits `IJsonService.LoadJson()`, which returns a `Tuple<bool, List<Test>>`.
3. If the first item is `false` (the configured directory does not exist), logs a warning and returns; the driver is still stopped by the surrounding `finally`.
4. Flattens the loaded tests with `TestsLoadedHandler.Flatten`.
5. For each `TestExecutionStep`, in order: reports it with `ILogger.ActionStarted`, resolves the matching `ITestEvent` through `IEventRegistry.Get(step.Step.Type)`, awaits `ITestEvent.ExecuteAsync(step)`, and reports the returned `TestExecutionResult` with `ILogger.ActionCompleted` on success or `ILogger.ActionFailed(step, result.Error)` on failure.

`StopAsync()` runs in a `finally` block, so the driver is stopped whether the load fails, an event throws, or the run completes.

## Responsibilities

- `JsonService`: reads every `*.json` file under `Projects:RootDirectory`, deserializes each into a `Test` with case-insensitive property matching, and returns the loaded `List<Test>`. It does not flatten, execute, or know about `TestExecutionResult`.
- `TestsLoadedHandler`: stateless. Its single method `Flatten` converts a `List<Test>` into `IEnumerable<TestExecutionStep>`:

  ```csharp
  tests.SelectMany(test => test.Workflows
      .SelectMany(workflow => workflow.Steps
          .Select(step => new TestExecutionStep(test, workflow, step))));
  ```

- `TestExecutionStep`: a record in `Domain.Entities.Execution` holding the `Test`, `Workflow`, and `TestSteps` instance produced by the flatten. It describes what should be executed.
- `IEventRegistry` (`EventService`): resolves an action name (`step.Step.Type`) to its `ITestEvent`. `EventService` is built from every registered `ITestEvent`, keyed by the `[Action(...)]` attribute name, case-insensitively. An unknown action throws `InvalidOperationException`.
- `ITestEvent`: executes one step and returns a `TestExecutionResult`. `ClickEvent` is the only implementation today; it is registered with `[Action("click")]`. Inside `ExecuteAsync`, an event resolves the step's logical element through `IElementRepository` and `ILocatorResolver`, then reports the result. The event itself holds no XPath, application knowledge, or platform types. See [Elements and Locators](./elements-and-locators.md).
- `TestExecutionResult`: describes the outcome of one executed step (`IsSuccess`, `Error`, `Locator`).
- `RunService`: orchestrates the run. It loads, flattens, resolves, executes, and reports. It depends on `IEventRegistry`, not on concrete events, and owns the driver lifecycle.
- `IAutomationDriver`: `StartAsync` before the load, `StopAsync` in a `finally`. See [Automation Driver Contract](./automation-driver-contract.md).
- `ILogger`: reports each step and its result. See [Logging](./logging.md).

## Execution order

Steps run sequentially, in the order the flatten produces them (test, then workflow, then step). Execution does not stop after the first step; every step is resolved and executed. There is no parallelism today.

## Registration

`AddServices` in `Infrastructure.Extensions.DependencyInjection` registers the logging services and the runtime services, all singletons: `ILogger` (`Logger`), `IPrintStrategy` (`ConsolePrintStrategy`), `IJsonService` (`JsonService`), `TestsLoadedHandler`, `IRunService` (`RunService`), and `IEventRegistry` (`EventService`). It then calls `AddEvents`, which registers each `ITestEvent` implementation (`ClickEvent` today) so `EventService` can index them.

## Related pages

- [Domain Layer](./domain-layer.md)
- [Dependency Injection](./dependency-injection.md)
- [Elements and Locators](./elements-and-locators.md)
- [Configuration](./configuration.md)
- [Logging](./logging.md)
