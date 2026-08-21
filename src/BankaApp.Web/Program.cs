using BankaApp.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<BankaApp.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = (builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5088").TrimEnd('/') + "/";

// Singleton: HttpClient handlers run in a separate DI scope; Scoped AuthSession
// would keep a stale JWT after login/logout and cause "self transfer" / wrong wallet bugs.
builder.Services.AddSingleton<AuthSession>();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient<WalletApi>(client =>
{
    client.BaseAddress = new Uri(apiBase);
}).AddHttpMessageHandler<AuthHeaderHandler>();

await builder.Build().RunAsync();
