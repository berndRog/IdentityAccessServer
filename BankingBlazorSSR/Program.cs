using BankingBlazorSSR.Hosting;
using BankingBlazorSSR.Hosting.Clients;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
namespace BankingBlazorSSR;

public sealed class Program {
   public static void Main(string[] args) {
      var builder = WebApplication.CreateBuilder(args);

      // ----------------------------
      // Logging
      // ----------------------------
      builder.Logging.ClearProviders();
      builder.Logging.AddConsole();
      builder.Logging.AddDebug();

      
      ConfigureServices(builder);

      var app = builder.Build();

      ConfigurePipeline(app);

      app.Run();
   }

   private static void ConfigureServices(WebApplicationBuilder builder) {
      
      builder.Services.AddHttpContextAccessor();
      builder.Services.AddHttpLogging(o => {
         o.LoggingFields =
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.RequestQuery |
            HttpLoggingFields.RequestHeaders |
            HttpLoggingFields.ResponseStatusCode |
            HttpLoggingFields.ResponseHeaders;

         // optional: Bodies (nur DEV, Achtung: kann sensibel sein)
         o.LoggingFields |= HttpLoggingFields.RequestBody |
            HttpLoggingFields.ResponseBody;

         o.RequestHeaders.Add("Authorization"); // Achtung: Token wird geloggt (DEV ok, PROD nein)
         o.MediaTypeOptions.AddText("application/json");
      });

      
      // Blazor SSR + Interactive Server
      builder.Services
         .AddRazorComponents()
         .AddInteractiveServerComponents();

      // Controllers für /login, /logout
      builder.Services.AddControllers();

      // AuthZ / Auth State
      builder.Services.AddAuthorization();
      builder.Services.AddCascadingAuthenticationState();
      builder.Services.AddHttpContextAccessor();

      ConfigureOidc(builder.Services, builder.Configuration);
      ConfigureBankingApi(builder.Services, builder.Configuration);
   }

   private static void ConfigureOidc(IServiceCollection services, IConfiguration config) {
      var auth = config.GetSection("AuthServer");
      Console.WriteLine($"SSR ClientId={auth["ClientId"]}, SecretPresent={!string.IsNullOrWhiteSpace(auth["ClientSecret"])}");

      
      services.AddAuthentication(options => {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
         })
         .AddCookie()
         .AddOpenIdConnect(options => {
            options.Authority = auth["Authority"]!;
            options.ClientId = auth["ClientId"]!;
            options.ClientSecret = auth["ClientSecret"]!;
            options.ResponseType = OpenIdConnectResponseType.Code;

            options.CallbackPath = auth["CallbackPath"] ?? "/signin-oidc";
            options.SignedOutCallbackPath = auth["SignedOutCallbackPath"] ?? "/signout-callback-oidc";

            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            //options.Scope.Add("email");
            options.Scope.Add(auth["ApiScope"] ?? "banking_api");

            options.RequireHttpsMetadata = true;
         });
   }

   private static void ConfigureBankingApi(IServiceCollection services, IConfiguration config) {
      services.AddTransient<AccessTokenHandler>();

      services.AddHttpClient("BankingApi", client => { client.BaseAddress = new Uri(config["BankingApi:BaseUrl"]!); })
         .AddHttpMessageHandler<AccessTokenHandler>();

      services.AddScoped<OwnersClient>(sp => {
         var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("BankingApi");
         return new OwnersClient(http);
      });
   }

   private static void ConfigurePipeline(WebApplication app) {
      // Template-typisch
      if (!app.Environment.IsDevelopment()) {
         app.UseExceptionHandler("/Error", createScopeForErrors: true);
         app.UseHsts();
      }

      app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

      app.UseHttpsRedirection();

      // .NET 10 Template nutzt oft MapStaticAssets statt UseStaticFiles
      app.MapStaticAssets();

      app.UseAntiforgery();

      // Auth muss vor Endpoints
      app.UseAuthentication();
      app.UseAuthorization();

      // Controller Endpoints
      app.MapControllers();

      // Blazor
      app.MapRazorComponents<App>()
         .AddInteractiveServerRenderMode();
   }
}