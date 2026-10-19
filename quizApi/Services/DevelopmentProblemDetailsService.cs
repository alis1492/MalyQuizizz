// using System.Text.Json;
// using System.Text.Json.Serialization;
// using DnsClient.Protocol;

// namespace quizApi.Services;

// // RTC response factory for development to help frontend team
// public class DevelopmentProblemDetailsSerivice(
//     ILogger<DevelopmentProblemDetailsSerivice> logger
// ) : IProblemDetailsService
// {
//     private readonly ILogger<DevelopmentProblemDetailsSerivice> _logger = logger;
//     public async ValueTask WriteAsync(ProblemDetailsContext context)
//     {
//         _logger.LogInformation(JsonSerializer.Serialize(context.ProblemDetails, new JsonSerializerOptions{WriteIndented = true}));
//         var http = context.HttpContext;
//         await http.Response.WriteAsync(context.Exception.Message);
//     }
// }