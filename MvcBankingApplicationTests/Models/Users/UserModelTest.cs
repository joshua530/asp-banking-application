using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using MvcBankingApplication.Models.Users;
using Moq;


namespace MvcBankingApplicationTests.Models.Users;

[Ignore]
[TestClass]
public class UserModelTest
{
    [TestMethod]
    public void TestPasswordEncryption()
    {
        string password = "abcde";
        byte[] salt = GenerateSalt();
        string expected = HashPassword(password, salt);
        string actual = UserModel.EncryptPassword(password, salt);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestPasswordVerification()
    {
        string password = "abcde";
        byte[] salt = GenerateSalt();
        string hash = HashPassword(password, salt);
        Assert.IsTrue(UserModel.VerifyPassword(password, hash));
    }

    protected byte[] GenerateSalt()
    {
        byte[] salt = new byte[128 / 8];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
            return salt;
        }
    }

    protected string HashPassword(string password, byte[] salt)
    {
        byte[] hashedPassBytes = KeyDerivation.Pbkdf2(
                salt: salt, password: password,
                prf: KeyDerivationPrf.HMACSHA256, numBytesRequested: 256 / 8,
                iterationCount: 10000);
        string passwordHashStr = Convert.ToBase64String(hashedPassBytes);
        string saltStr = Convert.ToBase64String(salt);
        return $"{saltStr}:{passwordHashStr}";
    }
}
