using Microsoft.AspNetCore.Components.Server.Circuits;

namespace quizUi.Services;

public class ServicesAccessorCircuitHandler(
    IServiceProvider services,
    CircuitServicesAccessor accessor
) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
    {
        return async context =>
        {
            accessor.Services = services;
            await next(context);
            accessor.Services = null;
        };
    }
}