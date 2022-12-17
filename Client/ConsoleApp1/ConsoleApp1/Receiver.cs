using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using CipherOTP;

namespace Client
{
	public class Receiver
	{
        public static void ReceiveData(TcpClient client)
        {
            StreamReader sr = new StreamReader(client.GetStream(), Encoding.UTF8);
            string message;
            while ((message = sr.ReadLine()) != null)
            {
                //TODO rewrite ecnryption to server side
                if (Regex.IsMatch(message, "\\];\\["))
                {
                    string[] message_key = message.Split("];[");
                    //TODO error on index
                    string decryptResult = Crypt.DecryptXOR(message_key[2], Int32.Parse(message_key[3]));
                    Console.WriteLine(message_key[0] +" | "+message_key[1] +" ~ "+decryptResult);
                }
                else
                {
                    Console.Write(message);
                }
            }
        }
	}
}

