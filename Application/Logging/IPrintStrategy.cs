namespace Application.Logging;

public interface IPrintStrategy
{
    void Render(LogEntry entry);
}
