using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Security.Cryptography;

namespace dluhy6_0.Security;

class PEncryption
{
    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);


        if (PasswordChecker.CheckPassword(password))
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(256 / 8);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            return savedPasswordHash;
        }
        else
        {
            return "invalid password";
        }

    }
    public static bool VerifyPassword(string inputPassword, string savedPasswordHash)
    {
        /* Extract the bytes */
        byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
        /* Get the salt */
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);
        /* Compute the hash on the password the user entered */
        var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        /* Compare the results */
        for (int i = 0; i < 20; i++)
            if (hashBytes[i + 16] != hash[i])
                return false;
        return true;
    }


}
