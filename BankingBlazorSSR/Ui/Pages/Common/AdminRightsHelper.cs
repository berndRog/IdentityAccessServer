using System.Security.Claims;

namespace BankingBlazorSsr.Ui.Pages.Common;

public static class AdminRightsHelper {
   public const string AdminRightsClaimType = "admin_rights";
   public const int ManageEmployees = 1 << 6;

   public static int GetAdminRights(ClaimsPrincipal user) {
      var raw = user.FindFirst(AdminRightsClaimType)?.Value;
      return int.TryParse(raw, out var value) ? value : 0;
   }

   public static bool HasManageEmployeesRight(int adminRights)
      => (adminRights & ManageEmployees) == ManageEmployees;
}

