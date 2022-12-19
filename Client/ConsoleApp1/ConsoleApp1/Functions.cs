using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using CipherOTP;

namespace Client
{
    /// <summary>
    /// Class that contains all main function used in TCP Client
    /// </summary>
    public static class Functions
    {
        static StreamReader sr;
        static StreamWriter sw;

        /// <summary>
        /// Sents message to server with given TCP Client
        /// </summary>
        /// <param name="input"> Message to send </param>
        /// <param name="client"> Current TCP Client </param>
        public static void Write(string input, TcpClient client)
        {
            sw = new StreamWriter(client.GetStream(), Encoding.ASCII);
            (string message, int pointer) = Crypt.EncryptXOR(input);
            sw.WriteLine(message + "];[" + pointer);
            sw.Flush();
        }

        /// <summary>
        /// Reciever that listens to incoming data using TCP Client with StreamReader
        /// Reads and decrypts information if crypted
        /// </summary>
        /// <param name="client"> Current TCP Client of client </param>
        public static void ReceiveData(TcpClient client)
        {
            sr = new StreamReader(client.GetStream(), Encoding.ASCII);
            string message;
            try
            {
                while ((message = sr.ReadLine()) != null)
                {

                    if (Regex.IsMatch(message, "\\];\\["))
                    {
                        string[] message_key = message.Split("];[");

                        string decryptResult = Crypt.DecryptXOR(message_key[2], Int32.Parse(message_key[3]));

                        Console.WriteLine(message_key[0] + " | " + message_key[1] + " ~ " + decryptResult);
                    }
                    else
                    {
                        Console.WriteLine("");
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
