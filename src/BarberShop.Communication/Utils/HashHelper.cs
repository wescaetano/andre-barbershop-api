using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HH = BCrypt.Net.BCrypt;

namespace BarberShop.Communication.Utils
{
    public class HashHelper
    {
        public static string HashGeneration(string password)
        {
            string salt = HH.GenerateSalt(15);
            string hash = HH.HashPassword(password, salt);
            return hash;
        }

        public static bool PasswordCompare(string hash, string password) => HH.Verify(password, hash);

        /// <summary>
        /// Generates a simple and random password with the specified length.
        /// </summary>
        public static string GeneratePassWord(int lenght)
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool IsValidPassword(string password) =>
            System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).*$");
    }
}
