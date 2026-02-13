// namespace BankingBlazorSsr.Ui.Components.Layout;
//
// public partial class NavMenu {
//    
//    private bool collapseNavMenu = true;
//    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;
//
//    // Login mit ReturnUrl (kein /owner mehr - wird über Roles gesteuert)
//    private string LoginUrl =>
//       "/identity/login?returnUrl=" + Uri.EscapeDataString(Nav.ToBaseRelativePath(Nav.Uri).Insert(0, "/"));
//
//    // serverseitiger Logout (Cookie + OIDC)
//    private string LogoutUrl => "/identity/logout";
//
//    private void ToggleNavMenu() => collapseNavMenu = !collapseNavMenu;
// }
