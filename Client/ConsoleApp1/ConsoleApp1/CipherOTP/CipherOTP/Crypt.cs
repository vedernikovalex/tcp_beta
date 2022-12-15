using System;
using System.Security.Cryptography;
using HtmlAgilityPack;
using System.Configuration;

namespace CipherOTP
{
    public static class Crypt
    {
        static string url = ConfigurationManager.AppSettings.Get("url");
        static HtmlWeb website = new HtmlWeb();
        static HtmlDocument document = website.Load(url);

        static string main_key = document.DocumentNode.OuterHtml;
        //static string main_key = "AKJLSDHKSHDiuheirwrufkjvbc3824729480914pjlafm<<D>?<?<!@#)*(!%&*kdjhfjsls";

        /// <summary>
        /// Encrypts message by generating random pointer to key with OTP cipher
        /// </summary>
        /// <param name="input"> Message to encrypt </param>
        /// <returns> string - encrypted message; int - used pointer to key </returns>
        public static (string, int) EncryptXOR(string input)
        {
            string result = "";
            int pointer = RandomNumberGenerator.GetInt32(main_key.Length);
            int pointer_temp = pointer;
            for (int i = 0; i < input.Length; i++)
            {
                char temp = input[i];
                result += (char)(temp ^ main_key[pointer_temp]);
                if (pointer_temp + 1 < input.Length)
                {
                    pointer_temp += 1;
                }
                else
                {
                    pointer_temp = 0;
                }
            }
            return (result, pointer);
        }

        /// <summary>
        /// Decrypts OTP message by inverting steps of encryption using same pointer to key given
        /// </summary>
        /// <param name="input">Message to decrypt</param>
        /// <param name="pointer">Pointer given</param>
        /// <returns> string - decrypted message </returns>
        public static string DecryptXOR(string input, int pointer)
        {
            string result = "";
            for (int i = 0; i < input.Length; i++)
            {
                char temp = input[i];
                result += (char)(main_key[pointer] ^ temp);
                if (pointer + 1 < input.Length)
                {
                    pointer += 1;
                }
                else
                {
                    pointer = 0;
                }
            }
            return result;
        }

    }
}