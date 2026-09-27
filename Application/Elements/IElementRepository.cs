using SharedKernel.Elements;

namespace Application.Elements;

public interface IElementRepository
{
    Element Get(string attribute, string target);
}
