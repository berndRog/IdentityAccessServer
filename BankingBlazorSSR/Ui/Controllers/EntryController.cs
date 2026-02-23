using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankingBlazorSsr.Ui.Controllers;

[Authorize]
public sealed class EntryController(
   ILogger<EntryController> logger
) : Controller {
   [HttpGet("/entry")]
   public IActionResult Index() {
      if (User.IsInRole("Customer"))
         return Redirect("/customers/provision");

      if (User.IsInRole("Employee"))
         return Redirect("/employees/provision");

      logger.LogWarning("Entry: user has no supported role.");
      return Redirect("/no-access");
   }
}