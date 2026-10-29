using quizUi.Components;
using quizUi.Options;
using quizUi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configuration
builder.Services.AddOptions<QuizApiOptions>()
    .Bind(builder.Configuration.GetSection(QuizApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Custom Services
builder.Services.AddScoped<QuizStateService>();

// HttpClientFactory for api calls
builder.Services.AddHttpClient("QuizApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["QuizApiOptions:BaseUrl"]!);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
