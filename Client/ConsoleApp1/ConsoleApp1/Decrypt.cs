using System;
namespace Client
{
	static class Decrypt
	{
        public static string DecryptXOR(string input, string key)
        {
            string result = "";
            for (int i = 0; i < input.Length; i++)
            {
                char temp = input[i];
                char tempKey = key[i];
                result += (char)(tempKey ^ temp);
            }
            return result;
        }
    }
}

