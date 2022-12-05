using System;
using System.Security.Cryptography;

namespace Client
{
	static class Encrypt
	{

        public static (string, string) EncryptXOR(string input)
        {
            string result = "";
            string key = "";
            for (int i = 0; i < input.Length; i++)
            {
                char temp = input[i];
                char key_int = (char)RandomNumberGenerator.GetInt32(123);
                result += (char)(temp ^ key_int);
                char key_char = (char)key_int;
                key += key_char;
            }
            return (result, key);
        }
        
	}
}

