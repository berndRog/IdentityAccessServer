using BankingBlazorSsr.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingBlazorSsr.Ui.Controllers;

[Authorize]
public sealed class EntryController(
   ICustomerClient customerClient,
   IEmployeeClient employeeClient,
   ILogger<EntryController> logger
) : Controller {
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
         var result = await employeeClient.GetProfileAsync(ct);

         if (result.IsSuccess && result.Value is not null) {
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
}