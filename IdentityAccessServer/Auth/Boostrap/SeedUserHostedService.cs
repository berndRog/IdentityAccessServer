using IdentityAccessServer.Data;
using IdentityAccessServer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using static IdentityAccessServer.Data.AdminRights;
namespace IdentityAccessServer.Auth.Seeding;

/// <summary>
/// Seeds demo users for the course.
/// - customer@demo.local
/// - admin@demo.local (employee with AdminRights bitmask)
///
/// This is course/demo-only.
/// </summary>
public sealed class SeedUsersHostedService : IHostedService {
   
   private readonly IServiceProvider _sp;

   public SeedUsersHostedService(IServiceProvider sp) => _sp = sp;

   public async Task StartAsync(CancellationToken ct) {
      using var scope = _sp.CreateScope();
      var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
      
      // ----------------------------
      // Customer demo user
      // ----------------------------
      // await EnsureUserAsync(
      //    users,
      //    id: Guid.Parse("00000000-0000-0000-0001-000000000002"),
      //    email: "customer@mail.local",
      //    password: "Geh1m_",
      //    adminRights: AdminRights.None
      // );
      
      // ----------------------------
      // Owner demo user (starts as Pending)
      // ----------------------------
      await EnsureUserAsync(
         users,
         id: Guid.Parse("00000000-0000-0000-0001-000000000001"),
         email: "owner@mail.local",
         password: "Geh1m_",
         accountType: "owner",
         adminRights: AdminRights.None
      );

      // ----------------------------
      // Admin demo user (Employee)
      // Example rights: manage cars + bookings + customers + employees
      // ----------------------------
      await EnsureUserAsync(
         users,
         id: Guid.Parse("00000000-0000-0000-0002-000000000001"),
         email: "admin@mail.local",
         password: "Geh1m_",
         accountType: "employee",
         adminRights: (AdminRights) 511
      );
   }

   private static async Task EnsureUserAsync(
      UserManager<ApplicationUser> users,
      Guid id,
      string email,
      string password,
      string accountType = "customer",
      AdminRights adminRights = AdminRights.None
   ) {
      var existing = await users.FindByEmailAsync(email);
      if (existing is not null) return;

      var user = new ApplicationUser {
         Id = id.ToString(),
         UserName = email,
         Email = email,
         EmailConfirmed = true,
         AccountType = accountType,
         AdminRights = adminRights,
         
         CreatedAt = DateTime.UtcNow,
         UpdatedAt = DateTime.UtcNow
      };

      var result = await users.CreateAsync(user, password);
      if (!result.Succeeded) {
         var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
         throw new InvalidOperationException($"Failed to seed user '{email}': {errors}");
      }
   }

   public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

/*
DE:
- Seedet zwei Demo-User, damit Login sofort getestet werden kann.
- AdminRights werden als int Bitmaske gespeichert und später als Claim ausgegeben.
*/