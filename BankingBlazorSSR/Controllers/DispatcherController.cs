using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingBlazorSsr.Controllers;

[AllowAnonymous]
public sealed class DispatcherController(
   ILogger<DispatcherController> logger
) : Controller {
   [HttpGet("/dispatcher")]
   public IActionResult Dispatch() {
      if (User?.Identity?.IsAuthenticated != true) {
         logger.LogInformation("Dispatcher not authenticated redirecting to login");
         return Redirect("/identity/login?returnUrl=%2Fdispatcher");
      }
      if (User.IsInRole("Owner")) {
         logger.LogInformation("Dispatcher redirecting to owner");
         return Redirect("/owner");
      }

      if (User.IsInRole("Employee")) {
         logger.LogInformation("Dispatcher redirecting to employee");
         return Redirect("/employee");
      }

      logger.LogWarning("Dispatcher user {User} has no recognized role, redirecting to no-access", User.Identity.Name);
      
      return Redirect("/no-access");
   }
}