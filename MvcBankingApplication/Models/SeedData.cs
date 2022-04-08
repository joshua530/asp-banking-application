using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using MvcBankingApplication.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace MvcBankingApplication.Models;

/// <summary>
/// seeds empty database with data
/// </summary>
public class SeedData
{
    public static async Task Initialize(IServiceProvider provider)
    {
        using (
            var context = new ApplicationContext(
                provider.GetRequiredService<DbContextOptions<ApplicationContext>>()))
        {
            // data has already been seeded
            if (context.Cashiers.Any())
            {
                return;
            }

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            //########### roles #################
            IdentityRole R_Customer = new IdentityRole("customer");
            IdentityRole R_Admin = new IdentityRole("admin");
            IdentityRole R_Cashier = new IdentityRole("cashier");
            if (!await roleManager.RoleExistsAsync("customer"))
                await roleManager.CreateAsync(R_Customer);
            if (!await roleManager.RoleExistsAsync("admin"))
                await roleManager.CreateAsync(R_Admin);
            if (!await roleManager.RoleExistsAsync("cashier"))
                await roleManager.CreateAsync(R_Cashier);

            //########### users #################
            var cashier = new Users.Cashier
            {
                FirstName = "John",
                LastName = "Doe",
                TransactionLimit = 10000,
                Email = "johndoe@email.com",
                UserName = "johndoe",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(cashier, "abcdef*A2");
            await userManager.AddToRoleAsync(cashier, "cashier");

            var admin = new Users.Admin
            {
                FirstName = "Alice",
                LastName = "Doe",
                Email = "alicedoe@email.com",
                UserName = "alicedoe",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "abcdef*A2");
            await userManager.AddToRoleAsync(admin, "admin");

            var customer = new Users.Customer
            {
                FirstName = "Jane",
                LastName = "Doe",
                UserName = "janedoe",
                Email = "janedoe@email.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(customer, "abcdef*A2");
            await userManager.AddToRoleAsync(customer, "customer");

            //########### accounts #################
            context.CustomerAccounts.Add(
                new Accounts.CustomerAccount
                {
                    OverdraftLimit = 1000,
                    Balance = 20000,
                    CustomerId = customer.Id
                });

            context.BankCashAccount.Add(
                new Accounts.BankCashAccount
                {
                    Type = Accounts.AccountType.BankCashAccount,
                    Balance = 1000000000
                });

            context.BankOverdraftAccount.Add(
                new Accounts.BankOverdraftAccount
                {
                    Type = Accounts.AccountType.BankOverdraftAccount,
                    Balance = 2000000000
                });

            //########### notifications #################
            context.Notifications.Add(
                new Notifications.Notification
                {
                    Id = 1,
                    ApplicationUserId = cashier.Id,
                    Message = "The transaction id <b>23xdsf45</b> has been approved",
                    Type = (int)Notifications.NotificationTypes.SUCCESS
                }
            );

            context.Notifications.Add(
                new Notifications.Notification
                {
                    Id = 2,
                    ApplicationUserId = admin.Id,
                    Message = "Transaction id <b>sdf43x</b> needs approval. <a href='#'>approve it</a>",
                    Type = (int)Notifications.NotificationTypes.SUCCESS
                }
            );

            context.Notifications.Add(
                new Notifications.Notification
                {
                    Id = 3,
                    ApplicationUserId = customer.Id,
                    Message = "You overdraft request id <b>kn7sdfnk</b> has been approved, check your account to confirm that the transaction succeeded",
                    Type = (int)Notifications.NotificationTypes.SUCCESS
                }
            );

            context.SaveChanges();
        }
    }
}
