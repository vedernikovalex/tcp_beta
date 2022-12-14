using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Configuration;
using System.Collections.Specialized;

namespace Client
{
    class Program
    {
        static string ip_address = ConfigurationManager.AppSettings.Get("ipAddress");
        static int port = Int32.Parse(ConfigurationManager.AppSettings.Get("port"));
        static bool username = false;

        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            Tuple<string, int> messages;
            try
            {
                tcpClient.Connect(ip_address, port);
                Console.WriteLine("Server found!");
                Console.WriteLine("Connected to "+ip_address+ " with port "+port+"!");
                NetworkStream ns = tcpClient.GetStream();

                Thread thread = new Thread(o => Receiver.ReceiveData((TcpClient)o));
                thread.Start(tcpClient);

                string s;
                while (!string.IsNullOrEmpty(s = Console.ReadLine()))
                {
                    if (!username)
                    {
                        byte[] usernameBuffer = Encoding.ASCII.GetBytes(s);
                        ns.Write(usernameBuffer, 0, usernameBuffer.Length);
                        username = true;
                    }
                    try
                    {
                        //if (Regex.IsMatch(s, "^[a-zA-Z]"))
                        //TODO -> find a way to simulate network failure
                        if (s == "DROP")
                        {
                            Console.WriteLine("string");
                            NetworkController.Disable();
                        }
                        (string message, int pointer) = Crypt.EncryptXOR(s);

                        byte[] buffer = Encoding.ASCII.GetBytes(message + "];[" + pointer);
                        ns.Write(buffer, 0, buffer.Length);
  
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
                Console.ReadKey();
            }
            catch (SocketException e)
            {
                Console.WriteLine("server could not be found!");
                //TODO actions
            }
        }
    }
}