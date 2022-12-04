using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Client
{
    class Program
    {
        static string ip_address = "127.0.0.1";
        static int port = 7777;

        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ip_address, port);
            Console.WriteLine("connected!");
            NetworkStream ns = tcpClient.GetStream();

            Thread thread = new Thread(o => ReceiveData((TcpClient)o));
            thread.Start(tcpClient);

            string s;
            while (!string.IsNullOrEmpty((s = Console.ReadLine())))
            {
                try
                {
                    if (Regex.IsMatch(s, "^[a-zA-Z]"))
                    {
                        (string message, string stringKey) = EncryptOTP(s);

                        byte[] buffer = Encoding.ASCII.GetBytes(message+";"+stringKey);
                        ns.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        throw new IndexOutOfRangeException("This program accepts english alphabetic characters only!");
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    Console.WriteLine(e);
                }
            }
            

            tcpClient.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            tcpClient.Close();
            Console.WriteLine("disconnected!");
            Console.ReadKey();
        }

        static void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                string message = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
                Console.WriteLine("BUFFER: " + message);
                string[] message_key = message.Split(';');
                string decryptResult = DecryptOTP(message_key[0], message_key[1]);
                Console.WriteLine(decryptResult);
            }
        }
        
        static int AlphabetArrayRangeExtender(int index)
        {
            int positiveCount = 26;
            //int negativeCount = -26;
            while (index > positiveCount)
            {
                if (index > positiveCount)
                {
                    index -= positiveCount;
                }
                /*
                if (index < negativeCount)
                {
                    index -= negativeCount;
                }
                */
            }
            return index;
        }

        static (string, string) EncryptOTP(string input)
        {
            string message = "";
            string stringKey = "";
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < input.Length; i++)
            {
                int key = RandomNumberGenerator.GetInt32(26);
                char temp = input[i];
                int index = char.ToUpper(temp) - 65;
                int cipherNum = index + key;
                message += chars[AlphabetArrayRangeExtender(cipherNum)];
                stringKey += chars[key];
            }
            return (message, stringKey);
        }

        static string DecryptOTP(string input, string key)
        {
            string message = "";
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int step = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char temp = input[i];
                char tempKey = key[i];
                int message_int = char.ToUpper(temp) - 65;
                int key_int = char.ToUpper(tempKey) - 65;
                Console.WriteLine("Msg before" + message_int);
                Console.WriteLine("Key before" + key_int);
                if (message_int < key_int)
                {
                    step = key_int - message_int;
                    message_int = 26;
                    key_int = 26 - step;
                }
                /*
                if (message_int > 26)
                {
                    step = message_int - 26;
                    message_int = message_int - 26 + step;
                    key_int = key_int - 26 + step;
                }
                */
                Console.WriteLine("Msg after"+message_int);
                Console.WriteLine("Key after"+key_int);
                int result_int = message_int - key_int;
                Console.WriteLine(result_int);
                message += chars[result_int];
            }
            return message;
        }
    }
}