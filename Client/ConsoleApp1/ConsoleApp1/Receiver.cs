using System;
using System.Net.Sockets;
using System.Text;

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
                Console.WriteLine("BUFFER: " + message);
                string[] message_key = message.Split(';');
                string decryptResult = Decrypt.DecryptXOR(message_key[0], message_key[1]);
                Console.WriteLine(decryptResult);
            }
        }
        
	}
}

