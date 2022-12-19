using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using CipherOTP;

namespace Client
{
    public static class Functions
    {
        static StreamReader sr;
        static StreamWriter sw;
        public static void Write(string input, TcpClient client)
        {
            sw = new StreamWriter(client.GetStream(), Encoding.ASCII);
            (string message, int pointer) = Crypt.EncryptXOR(input);
            sw.WriteLine(message + "];[" + pointer);
            sw.Flush();
        }

        public static void ReceiveData(TcpClient client)
        {
            sr = new StreamReader(client.GetStream(), Encoding.ASCII);
            string message;
            try
            {
                while ((message = sr.ReadLine()) != null)
                {

                    //TODO rewrite ecnryption to server side
                    if (Regex.IsMatch(message, "\\];\\["))
                    {
                        string[] message_key = message.Split("];[");
                        //TODO error on index
                        string decryptResult = Crypt.DecryptXOR(message_key[2], Int32.Parse(message_key[3]));
                        Console.WriteLine(message_key[0] + " | " + message_key[1] + " ~ " + decryptResult);
                    }
                    else
                    {
                        Console.Write(message);
                    }
                }
                
            }
            catch (IOException e)
            {
                Console.WriteLine("\n Server has ended connection!");
                Console.WriteLine(e.Message);
                Console.WriteLine("Disconnected...");
                if (client.Connected)
                {
                    client.Client.Shutdown(SocketShutdown.Send);
                    client.Close();
                }
            }

        }
    }
}
