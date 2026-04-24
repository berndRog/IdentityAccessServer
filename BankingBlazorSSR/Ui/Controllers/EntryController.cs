using BankingBlazorSsr.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingBlazorSsr.Ui.Controllers;

[Authorize]
public sealed class EntryController(
   ICustomerClient customerClient,
   IEmployeeClient employeeClient,
   IConfiguration configuration,
   ILogger<EntryController> logger
) : Controller {
   private const string MustChangePasswordClaimType = "must_change_password";

   [HttpGet("/entry")]
   public async Task<IActionResult> Index(CancellationToken ct) {
      if (User.IsInRole("Customer")) {
         var result = await customerClient.GetProfileAsync(ct);

         if (result.IsSuccess && result.Value is not null) {
            return result.Value.Status == 1
               ? Redirect("/customer")
               : Redirect("/customers/profile?onboarding=true");
         }

         if (result.Error?.Status == 404)
            return Redirect("/customers/provision");

         logger.LogWarning("Entry(Customer): profile lookup failed with status {Status}. Falling back to provisioning.",
            result.Error?.Status);
         return Redirect("/customers/provision");
      }

      if (User.IsInRole("Employee")) {
         var mustChangePassword = User.FindFirst(MustChangePasswordClaimType)?.Value == "true";
         var result = await employeeClient.GetProfileAsync(ct);

         if (result.IsSuccess && result.Value is not null) {
            // Initial password changes are only enforced after employee onboarding
            // has completed and the Banking profile is already active.
            if (mustChangePassword && result.Value.IsActive)
               return Redirect(BuildChangeInitialPasswordUrl());

            return result.Value.IsActive
               ? Redirect("/employee")
               : Redirect("/employees/profile?onboarding=true");
         }

         if (result.Error?.Status == 404)
            return Redirect("/employees/provision");

         logger.LogWarning("Entry(Employee): profile lookup failed with status {Status}. Falling back to provisioning.",
            result.Error?.Status);
         return Redirect("/employees/provision");
      }

      logger.LogWarning("Entry: user has no supported role.");
      return Redirect("/no-access");
   }

   private string BuildChangeInitialPasswordUrl() {
      var authority = configuration["AuthServer:Authority"]?.TrimEnd('/');
      if (string.IsNullOrWhiteSpace(authority)) {
         logger.LogWarning("Entry(Employee): AuthServer:Authority missing. Falling back to employee dashboard.");
         return "/employee";
      }

      var returnUrl = Url.ActionLink(nameof(Index), "Entry", values: null, protocol: Request.Scheme)
         ?? $"{Request.Scheme}://{Request.Host}{Request.PathBase}/entry";

      return $"{authority}/Identity/Account/ChangeInitialPassword?returnUrl={Uri.EscapeDataString(returnUrl)}";
   }
}