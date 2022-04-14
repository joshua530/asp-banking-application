#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MvcBankingApplication.Models.Users;
using MvcBankingApplication.Models.Transactions;
using MvcBankingApplication.Models.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class ApplicationContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // set default image value
        builder.Entity<ApplicationUser>()
                .Property(e => e.ImageUrl)
                .HasDefaultValue("/images/users/avatar.png");
        // transactions
        builder.Entity<Transaction>()
            .HasOne(m => m.AccountCredited)
            .WithMany(ac => ac.CreditTransactions)
            .HasForeignKey(m => m.AccountCreditedId);
        builder.Entity<Transaction>()
            .HasOne(m => m.AccountDebited)
            .WithMany(ac => ac.DebitTransactions)
            .HasForeignKey(m => m.AccountDebitedId);

        base.OnModelCreating(builder);
    }

    public DbSet<MvcBankingApplication.Models.Users.Customer> Customers { get; set; }

    public DbSet<MvcBankingApplication.Models.Transactions.Transaction> Transactions { get; set; }

    public DbSet<MvcBankingApplication.Models.Users.Cashier> Cashiers { get; set; }

    public DbSet<MvcBankingApplication.Models.Users.Admin> Admins { get; set; }

    public DbSet<MvcBankingApplication.Models.Notifications.Notification> Notifications { get; set; }

    public DbSet<MvcBankingApplication.Models.Accounts.CustomerAccount> CustomerAccounts { get; set; }

    public DbSet<MvcBankingApplication.Models.Accounts.BankCashAccount> BankCashAccount { get; set; }

    public DbSet<MvcBankingApplication.Models.Accounts.BankOverdraftAccount> BankOverdraftAccount { get; set; }

    public DbSet<MvcBankingApplication.Models.Notifications.AdminNotification> AdminNotifications { get; set; }

    public DbSet<MvcBankingApplication.Models.Transactions.PendingTransaction> PendingTransactions { get; set; }
}
