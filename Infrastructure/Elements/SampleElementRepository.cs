using Application.Elements;

using SharedKernel.Components.Mui;
using SharedKernel.Components.Shadcn;
using SharedKernel.Components.Syncfusion;
using SharedKernel.Elements;
using SharedKernel.Locators;

namespace Infrastructure.Elements;

public sealed class SampleElementRepository : IElementRepository
{
    private readonly Dictionary<string, Element> _elements = new(StringComparer.OrdinalIgnoreCase);

    public SampleElementRepository()
    {
        Add("LoginButton", "LoginButton", MuiLocators.Button("Login", "login-button"));
        Add("UsernameInput", "Username", MuiLocators.TextField("Username", "username-input"));
        Add("PasswordInput", "Password", MuiLocators.TextField("Password", "password-input"));
        Add("RememberMeSwitch", "RememberMe", MuiLocators.Switch("Remember me", "remember-me"));

        Add("FirstNameInput", "FirstName", ShadcnLocators.Input("First name", "first-name-input"));
        Add("LastNameInput", "LastName", ShadcnLocators.Input("Last name", "last-name-input"));
        Add("EmailInput", "Email", ShadcnLocators.Input("Email", "email-input"));
        Add("RegisterButton", "RegisterButton", ShadcnLocators.Button("Register", "register-button"));

        Add("SearchInput", "SearchBox", SyncfusionLocators.Input("Search", "search-input"));
        Add("SearchButton", "SearchButton", SyncfusionLocators.Button("Search", "search-button"));
        Add("SearchResult", "SearchResult", SyncfusionLocators.Input("Search result", "search-result"));
    }

    public Element Get(string attribute, string target)
    {
        if (TryGet(target, out var byTarget))
        {
            return byTarget;
        }

        if (TryGet(attribute, out var byAttribute))
        {
            return byAttribute;
        }

        throw new InvalidOperationException(
            $"No element is defined for attribute '{attribute}' or target '{target}'.");
    }

    private void Add(string target, string attribute, IReadOnlyList<Locator> locators)
    {
        var element = new Element(target, locators);
        _elements[target] = element;
        _elements.TryAdd(attribute, element);
    }

    private bool TryGet(string key, out Element element)
    {
        if (!string.IsNullOrWhiteSpace(key) && _elements.TryGetValue(key, out var found))
        {
            element = found;
            return true;
        }

        element = null!;
        return false;
    }
}
