using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using WebClientMvc;
using WebClientMvc.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAccessTokenProvider, AccessTokenProviderFromHttpContext>();

builder.Services
   .AddOptions<OidcClientOptions>()
   .Bind(builder.Configuration.GetSection("AuthServer"))
   .ValidateDataAnnotations()
   .ValidateOnStart();

builder.Services.AddHttpClient<ICustomersApiClient, CustomersApiClient>(c => {
   c.BaseAddress = new Uri("https://localhost:8010/carrentalapi/v1/");
});


var oidc = builder.Configuration
   .GetSection("AuthServer")
   .Get<OidcClientOptions>()!;

var clientSecret  = builder.Configuration["AuthServer:WebMvc:ClientSecret"] ?? "webclient-mvc-secret"; // ✅ dein secret-key

Console.WriteLine($"AuthServer:Authority={oidc.Authority}");
Console.WriteLine($"AuthServer:ClientId={oidc.ClientId}");
Console.WriteLine($"AuthServer:WebMvc:ClientSecret={clientSecret}");
Console.WriteLine($"BaseUrl: {oidc.BaseUrl}");
Console.WriteLine($"RedirectPath: {oidc.RedirectPath}");
Console.WriteLine($"SignedOutCallbackPath: {oidc.SignedOutCallbackPath}");
Console.WriteLine($"PostLogoutRedirectPath: {oidc.PostLogoutRedirectPath}");

Console.WriteLine($"Scopes: {string.Join(", ", oidc.Scopes)}");


builder.Services.AddAuthentication(authOpt => {
      authOpt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      authOpt.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
   })
   .AddCookie()
   .AddOpenIdConnect(openIdOpt => {
      openIdOpt.Authority = oidc.Authority;
      openIdOpt.ClientId = oidc.ClientId;
      openIdOpt.ClientSecret = clientSecret;
      openIdOpt.ResponseType = "code";
     
      openIdOpt.SaveTokens = true;
      openIdOpt.GetClaimsFromUserInfoEndpoint = false;
      
      openIdOpt.CallbackPath = oidc.RedirectPath;
      openIdOpt.SignedOutCallbackPath = oidc.SignedOutCallbackPath;
      openIdOpt.SignedOutRedirectUri = oidc.PostLogoutRedirectUri.ToString();
      
      openIdOpt.Scope.Clear();
      foreach (var s in oidc.Scopes)
         openIdOpt.Scope.Add(s);
   });

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.Run();