using System.Net.Http.Headers;

namespace quizUi.Services;

public class AuthHeaderHandler(CircuitServicesAccessor circuitServices) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenService = circuitServices.Services?.GetService<TokenService>();

        if (!string.IsNullOrEmpty(tokenService?.Token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenService.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}