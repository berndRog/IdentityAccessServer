using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebClientMvc.Models.Dtos;
using WebClientMvc.Services;
namespace WebClientMvc.Controllers;

[Authorize]
[Route("customer")]
public sealed class CustomerController(
   ICustomersApiClient _api,
   ILogger<CustomerController> _logger
) : Controller {
   // ------------------------------------------------------------
   // GET /customer/provisioned
   // ------------------------------------------------------------
   // Called after login (or manually).
   // Creates a "raw customer" if not existing yet and redirects
   // to the profile page.
   [HttpGet("provisioned")]
   public async Task<IActionResult> Provisioned(CancellationToken ct) {
      var result = await _api.EnsureProvisionedAsync(ct);

      if (!result.IsSuccess) {
         if (result.StatusCode is 401 or 403)
            return Challenge();

         _logger.LogWarning(
            "Customer provisioning failed: {Title} {Detail}",
            result.Problem?.Title,
            result.Problem?.Detail
         );

         ViewBag.ErrorTitle = result.Problem?.Title ?? "Provisioning failed";
         ViewBag.ErrorDetail = result.Problem?.Detail;
         return View("Error");
      }

      // success → continue with profile completion
      return RedirectToAction("Profile");
   }

   // ------------------------------------------------------------
   // GET /customer/profile
   // ------------------------------------------------------------
   [HttpGet("profile")]
   public async Task<IActionResult> Profile(CancellationToken ct) {
      
      var profile = await _api.GetProfileAsync(ct);
      
      // Not found → redirect to provisioning
      if (profile.StatusCode == 404)
         return RedirectToAction("Provisioned");

      // Other errors
      if (!profile.IsSuccess || profile.Value is null) {
         if (profile.StatusCode is 401 or 403)
            return Challenge();

         _logger.LogWarning(
            "Loading customer profile failed: {Title} {Detail}",
            profile.Problem?.Title,
            profile.Problem?.Detail
         );

         ViewBag.ErrorTitle = profile.Problem?.Title ?? "Loading profile failed";
         ViewBag.ErrorDetail = profile.Problem?.Detail;
         return View("Error");
      }

      return View(profile.Value);
   }

   // ------------------------------------------------------------
   // POST /customer/profile
   // ------------------------------------------------------------
   [HttpPost("profile")]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> Profile(
      CustomerProfileDto dto,
      CancellationToken ct
   ) {
      if (!ModelState.IsValid)
         return View(dto);

      var result = await _api.UpdateProfileAsync(dto, ct);

      if (!result.IsSuccess || result.Value is null) {
         if (result.StatusCode is 401 or 403)
            return Challenge();

         ModelState.AddModelError(
            "",
            result.Problem?.Detail ?? result.Problem?.Title ?? "Profile update failed"
         );

         return View(dto);
      }

      return RedirectToAction("Index", "Home");
   }
}