using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebClientBlazorWasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Config aus wwwroot/appsettings.json
var authSection = builder.Configuration.GetSection("AuthServer");
var authority = authSection["Authority"] ?? "https://localhost:7001";
var clientId = authSection["ClientId"] ?? "blazor-wasm";
var scopes = authSection.GetSection("Scopes").Get<string[]>() ?? new[] { "openid" };

// Default HttpClient
builder.Services.AddHttpClient("WebClientBlazorWasm",
   client => { client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); });

// OIDC
// builder.Services.AddOidcAuthentication(options => {
//    options.ProviderOptions.Authority = authority.TrimEnd('/');
//    options.ProviderOptions.ClientId = clientId;
//    options.ProviderOptions.ResponseType = "code"; // Code + PKCE
//    
//    options.ProviderOptions.DefaultScopes.Clear();
//    foreach (var scope in scopes) {
//       if (!string.IsNullOrWhiteSpace(scope))
//          options.ProviderOptions.DefaultScopes.Add(scope.Trim());
//    }
// });

builder.Services.AddOidcAuthentication(options => {
   options.ProviderOptions.Authority = "https://localhost:7001/";
   options.ProviderOptions.ClientId = "blazor-wasm";
   options.ProviderOptions.ResponseType = "code";

   options.ProviderOptions.DefaultScopes.Clear();
   options.ProviderOptions.DefaultScopes.Add("openid");
   options.ProviderOptions.DefaultScopes.Add("profile");
   options.ProviderOptions.DefaultScopes.Add("api");

   // Fürs Debuggen erstmal aus:
   //options.ProviderOptions.AutomaticSilentRenew = false;
});

System.Console.WriteLine("WebClientBlazorWasm starting...");
System.Console.WriteLine($"AuthServer:Authority={authority}");
System.Console.WriteLine($"AuthServer:ClientId={clientId}");
System.Console.WriteLine($"Scopes: {string.Join(", ", scopes)}");

await builder.Build().RunAsync();