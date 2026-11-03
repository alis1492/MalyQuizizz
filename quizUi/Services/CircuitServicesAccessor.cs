namespace quizUi.Services;

public class CircuitServicesAccessor
{
    private static readonly AsyncLocal<IServiceProvider> _services = new();

    public IServiceProvider? Services
    {
        get => _services.Value;
        set => _services.Value = value;
    }
}