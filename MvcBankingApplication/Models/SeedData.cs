using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace MvcBankingApplication.Models;

/// <summary>
/// seeds empty database with data
/// </summary>
public class SeedData
{
    public static void Initialize(IServiceProvider provider)
    {
        using (
            var context = new ApplicationContext(
                provider.GetRequiredService<DbContextOptions<ApplicationContext>>()))
        {
            // data has already been seeded
            if (context.CashierModel.Any())
            {
                return;
            }

            //########### users #################
            context.CashierModel.Add(
                new Users.CashierModel
                {
                    ID = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    TransactionLimit = 10000,
                    StaffId = "JD343434",
                    Password = "abcdef",
                    Email = "johndoe@email.com",
                    Username = "johndoe"
                }
            );

            context.AdminModel.Add(
                new Users.AdminModel
                {
                    ID = 2,
                    FirstName = "Alice",
                    LastName = "Doe",
                    StaffId = "AD43",
                    Password = "abcdef",
                    Email = "alicedoe@email.com",
                    Username = "alicedoe",
                    IsAdmin = true
                }
            );

            context.CustomerModel.Add(
                new Users.CustomerModel
                {
                    ID = 3,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Username = "janedoe",
                    Email = "janedoe@email.com",
                    Password = "abcdef",
                }
            );

            //########### notifications #################
            context.NotificationModel.Add(
                new Notifications.NotificationModel
                {
                    Id = 1,
                    UserId = 1,
                    Message = "The transaction id <b>23xdsf45</b> has been approved",
                    Type = (int)Notifications.NotificationTypes.SUCCESS
                }
            );

            context.NotificationModel.Add(
                new Notifications.NotificationModel
                {
                    Id = 2,
                    UserId = 2,
                    Message = "Transaction id <b>sdf43x</b> needs approval. <a href='#'>approve it</a>",
                    Type = (int)Notifications.NotificationTypes.SUCCESS
                }
            );

            context.NotificationModel.Add(
                new Notifications.NotificationModel
                {
                    Id = 3,
                    UserId = 3,
                    Message = "You overdraft request id <b>kn7sdfnk</b> has been approved, check your account to confirm that the transaction succeeded",
                    Type = (int)Notifications.NotificationTypes.SUCCESS
                }
            );

            //########### accounts #################
            context.CustomerAccountModel.Add(
                new Accounts.CustomerAccountModel
                {
                    OverdraftLimit = 1000,
                    ID = 1,
                    UserId = 3,
                    Type = (int)Accounts.AccountType.CustomerAccount,
                    AccountNumber = 123,
                    Balance = 20000
                }
            );

            context.BankCashAccountModel.Add(
                new Accounts.BankCashAccountModel
                {
                    ID = 2,
                    Type = (int)Accounts.AccountType.BankCashAccount,
                    AccountNumber = 10001,
                    Balance = 1000000000
                }
            );

            context.BankOverdraftAccountModel.Add(
                new Accounts.BankOverdraftAccountModel
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
