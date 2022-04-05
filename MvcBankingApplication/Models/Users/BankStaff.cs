namespace MvcBankingApplication.Models.Users
{
    abstract public class BankStaff : ApplicationUser
    {
        public string StaffId { get; set; } = String.Empty;
        public bool IsAdmin { get; set; } = false;
    }
}
