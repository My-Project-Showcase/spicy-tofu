namespace Application.Automation.Web;

public interface IWebSession
{
    string Name { get; }

    IWebPage Page { get; }
}