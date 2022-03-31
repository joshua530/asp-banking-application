namespace MvcBankingApplication.Models.Users
{
    abstract public class BankUserModel : UserModel
    {
        public string StaffId { get; set; } = String.Empty;
        public bool IsAdmin { get; set; } = false;
    }
}
