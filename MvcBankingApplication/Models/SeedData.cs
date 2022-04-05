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
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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
                StaffId = "JD343434",
                Email = "johndoe@email.com",
                UserName = "johndoe"
            };
            await userManager.CreateAsync(cashier, "abcdef");
            await userManager.AddToRoleAsync(cashier, "cashier");

            var admin = new Users.Admin
            {
                FirstName = "Alice",
                LastName = "Doe",
                StaffId = "AD43",
                Email = "alicedoe@email.com",
                UserName = "alicedoe",
                IsAdmin = true
            };
            await userManager.CreateAsync(admin, "abcdef");
            await userManager.AddToRoleAsync(admin, "admin");

            var customer = new Users.Customer
            {
                FirstName = "Jane",
                LastName = "Doe",
                UserName = "janedoe",
                Email = "janedoe@email.com",
            };
            await userManager.CreateAsync(customer, "abcdef");
            await userManager.AddToRoleAsync(customer, "customer");


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

            //########### accounts #################
            context.CustomerAccounts.Add(
                new Accounts.CustomerAccount
                {
                    OverdraftLimit = 1000,
                    ID = 1,
                    CustomerId = customer.Id,
                    Type = (int)Accounts.AccountType.CustomerAccount,
                    AccountNumber = 123,
                    Balance = 20000
                }
            );

            context.BankCashAccount.Add(
                new Accounts.BankCashAccount
                {
                    ID = 2,
                    Type = (int)Accounts.AccountType.BankCashAccount,
                    AccountNumber = 10001,
                    Balance = 1000000000
                }
            );

            context.BankOverdraftAccount.Add(
                new Accounts.BankOverdraftAccount
                {
                    ID = 3,
                    Type = (int)Accounts.AccountType.BankOverdraftAccount,
                    AccountNumber = 10002,
                    Balance = 2000000000
                }
            );

            context.SaveChanges();
        }
    }
}
