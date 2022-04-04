#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.Transactions;
using MvcBankingApplication.Models.Notifications;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public DbSet<MvcBankingApplication.Models.Users.CustomerModel> CustomerModel { get; set; }

    public DbSet<MvcBankingApplication.Models.Transactions.TransactionModel> TransactionModel { get; set; }

    public DbSet<MvcBankingApplication.Models.Users.CashierModel> CashierModel { get; set; }

    public DbSet<MvcBankingApplication.Models.Users.AdminModel> AdminModel { get; set; }

    public DbSet<MvcBankingApplication.Models.Notifications.NotificationModel> NotificationModel { get; set; }

    public DbSet<MvcBankingApplication.Models.Accounts.CustomerAccountModel> CustomerAccountModel { get; set; }

    public DbSet<MvcBankingApplication.Models.Accounts.BankCashAccountModel> BankCashAccountModel { get; set; }

    public DbSet<MvcBankingApplication.Models.Accounts.BankOverdraftAccountModel> BankOverdraftAccountModel { get; set; }
}
