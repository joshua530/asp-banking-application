using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using MvcBankingApplication.Models.Users;
using System.Collections.Generic;
using System;

namespace MvcBankingApplicationTests.Models.Users.Validators;


//TODO how is custom validator tested?
[Ignore]
[TestClass]
public class ValidPasswordTest
{
    MockUser User = new MockUser
    {
        Username = "abc",
        FirstName = "abc",
        LastName = "abc"
    };

    [TestMethod]
    public void TestValidPassword()
    {
        ValidationContext ctxt = new ValidationContext(User);
        List<ValidationResult> results = new List<ValidationResult>();
        User.Password = "validPass3?";
        bool isValid = Validator.TryValidateObject(User, ctxt, results);
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void TestShortPassword()
    {
        User.Password = "pass?";
        ValidationContext ctxt = new ValidationContext(User);
        List<ValidationResult> results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(User, ctxt, results);
        foreach (var x in results)
        {
            Console.WriteLine(x);
        }
        Assert.IsFalse(isValid);
    }

    private class MockUser : UserModel { }
}
