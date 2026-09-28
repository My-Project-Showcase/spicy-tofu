using Application.Elements;
using Application.Logging;
using Application.Locators;

using Domain.Entities.Execution;
using Domain.Events;

using Action = SharedKernel.Attributes.ActionAttribute;

namespace Infrastructure.Events;

[Action("fill")]
public sealed class FillEvent : ITestEvent
{
    private readonly IElementRepository _elementRepository;
    private readonly ILocatorResolver _locatorResolver;
    private readonly ILogger _logger;

    public FillEvent(
        IElementRepository elementRepository,
        ILocatorResolver locatorResolver,
        ILogger logger)
    {
        _elementRepository = elementRepository;
        _locatorResolver = locatorResolver;
        _logger = logger;
    }

    public async Task<TestExecutionResult> ExecuteAsync(TestExecutionStep step)
    {
        try
        {
            var element = _elementRepository.Get(
                step.Step.Attribute ?? string.Empty,
                step.Step.Target ?? string.Empty);

            try
            {
                var resolved = await _locatorResolver.ResolveAsync(element);
                _logger.LocatorResolution(step, element.Locators, resolved.Locator);

                await resolved.FillAsync(step.Step.Value ?? string.Empty);

                return new TestExecutionResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LocatorResolution(step, element.Locators);
                return new TestExecutionResult { IsSuccess = false, Error = ex.Message };
            }
        }
        catch (InvalidOperationException ex)
        {
            return new TestExecutionResult { IsSuccess = false, Error = ex.Message };
        }
    }
}
