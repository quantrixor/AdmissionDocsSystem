using System;
using System.Linq;
using System.Security.Cryptography;

namespace AdmissionDocsSystem.ViewModel
{
    public class PasswordGenerator
    {
        private const string LowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string SpecialCharacters = "!@#$%&";

        public static string GeneratePassword(int length = 8)
        {
            if (length < 4) throw new ArgumentException("Password length must be at least 4 characters.", nameof(length));

            // Создаем массив для хранения пароля
            char[] password = new char[length];
            Random random = new Random();

            // Гарантировать, что в пароле будут все типы символов
            password[0] = LowerCaseLetters[random.Next(LowerCaseLetters.Length)];
            password[1] = UpperCaseLetters[random.Next(UpperCaseLetters.Length)];
            password[2] = Digits[random.Next(Digits.Length)];
            password[3] = SpecialCharacters[random.Next(SpecialCharacters.Length)];

            // Заполняем оставшиеся места случайными символами из всех категорий
            string allCharacters = LowerCaseLetters + UpperCaseLetters + Digits + SpecialCharacters;
            for (int i = 4; i < length; i++)
            {
                password[i] = allCharacters[random.Next(allCharacters.Length)];
            }

            // Перемешиваем символы для увеличения случайности
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }

}
