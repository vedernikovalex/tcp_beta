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
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                string message = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
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

