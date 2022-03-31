using System.ComponentModel.DataAnnotations;
using System;

namespace MvcBankingApplication.Models.Users
{
    abstract public class UserModel
    {
        public string ImageUrl { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string Username { get; set; } = String.Empty;
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public int Id { get; }
    }
}
