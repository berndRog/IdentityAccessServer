using System.Text;
using System.Text.Json;
using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
namespace BankingBlazorSsr.Ui.Pages.Customer;

public partial class CustomerProvisonPage : IDisposable {

   [Inject] private ICustomerClient CustomerClient { get; set; } = default!;
   [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
   [Inject] private NavigationManager NavigationManager { get; set; } = default!;
   [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
   [Inject] private ILogger<CustomerProvisonPage> Logger { get; set; } = default!;

   private readonly CancellationTokenSource _cts = new();

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   // Bind this in the .razor file to display claims
   private string? _idToken;
   private string? _accessToken;
   private List<(string Key, string Value)> _idTokenLines = [];
   private List<(string Key, string Value)> _accessTokenLines = [];
   private List<(string Type, string Value)> _idTokenClaims = [];

   private ProvisionDto _provision = default!;

   protected override async Task OnInitializedAsync() {
      var ctx = $"{nameof(CustomerProvisonPage)}.{nameof(OnInitializedAsync)}";

      Loading = true;
      ErrorMessage = null;
      Logger.LogInformation("{ctx}", ctx);

      try {
         var ct = _cts.Token;

         // Get tokens and decode them to display in the UI for demonstration purposes.
         var http = HttpContextAccessor.HttpContext;
         if (http is null) {
            ErrorMessage = "No HttpContext available.";
            return;
         }

         _idToken = await http.GetTokenAsync("id_token");
         _accessToken = await http.GetTokenAsync("access_token");
         _idTokenLines = DecodeJwtToLines(_idToken);
         _accessTokenLines = DecodeJwtToLines(_accessToken);

         // 1) ClaimsPrincipal from Blazor auth state
         var authState = await AuthStateProvider.GetAuthenticationStateAsync();
         var user = authState.User;

         Logger.LogInformation("User authenticated: {Auth}", user.Identity?.IsAuthenticated == true);

         _idTokenClaims = user.Identity?.IsAuthenticated == true
            ? user.Claims.Select(c => (c.Type, c.Value)).ToList()
            : [];

         // 2) Provision
         Logger.LogInformation("User PostPrivison");
         var resultProvision = await CustomerClient.PostProvisionAsync(ct);
         if (resultProvision.IsFailure) {
            HandleError(resultProvision.Error!);
            return;
         }
         _provision = resultProvision.Value!;
         Logger.LogInformation("Customer provisioned {value}",_provision);

         NavigationManager.NavigateTo("/customers/profile?onboarding=true", replace: true);
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("CustomerProvisonPage: request was cancelled");
      }
      catch (Exception ex) {
         ErrorMessage = "Unexpected error during provisioning.";
         Logger.LogError(ex, "CustomerProvisonPage: unexpected error");
      }
      finally {
         Loading = false;
         await InvokeAsync(StateHasChanged);
      }
   }


   private static List<(string Key, string Value)> DecodeJwtToLines(string? jwt) {
      var result = new List<(string, string)>();

      if (string.IsNullOrWhiteSpace(jwt)) {
         result.Add(("token", "(missing)"));
         return result;
      }

      var parts = jwt.Split('.');
      if (parts.Length < 2) {
         result.Add(("token", "invalid"));
         return result;
      }

      var payload = parts[1];
      payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
      payload = payload.Replace('-', '+').Replace('_', '/');

      var bytes = Convert.FromBase64String(payload);
      var json = Encoding.UTF8.GetString(bytes);

      using var doc = JsonDocument.Parse(json);

      foreach (var prop in doc.RootElement.EnumerateObject()) {
         result.Add((prop.Name, prop.Value.ToString()));
      }

      return result;
   }
}