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

            Thread thread = new Thread(o => Receiver.ReceiveData((TcpClient)o));
            thread.Start(tcpClient);

            string s;
            while (!string.IsNullOrEmpty((s = Console.ReadLine())))
            {
                try
                {
                    if (Regex.IsMatch(s, "^[a-zA-Z]"))
                    {
                        (string message, string stringKey) = Encrypt.EncryptXOR(s);

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


        /*
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
                int result_int = message_int - key_int;
                //result_int = result_int < 0 ? result_int * -1 : result_int;
                Console.WriteLine(result_int);
                message += chars[result_int];
            }
            return message;
        }
        */
    }
}