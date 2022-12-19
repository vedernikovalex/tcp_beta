using System.Text;
using System.Net.Sockets;
using System.Configuration;
using CipherOTP;
using System.Text.RegularExpressions;

namespace Client
{
    class Program
    {
        static string ip_address = ConfigurationManager.AppSettings.Get("ipAddress");
        static int port = Int32.Parse(ConfigurationManager.AppSettings.Get("port"));

        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            //List<string> messages = new List<string>();

            try
            {
                tcpClient.Connect(ip_address, port);
                Console.WriteLine("Server found!");
                Console.WriteLine("Connected to " + ip_address + " with port " + port + "!");

                NetworkStream ns = tcpClient.GetStream();

                Thread thread = new Thread(o => Functions.ReceiveData((TcpClient)o));
                thread.Start(tcpClient);

                string input;
                while (!string.IsNullOrEmpty(input = Console.ReadLine()))
                {
                    try
                    {
                        Console.WriteLine("");
                        Functions.Write(input, tcpClient);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        Console.WriteLine(e);
                    }

                }
                if (tcpClient.Connected)
                {
                    Console.WriteLine("Disconnected...");
                    tcpClient.Client.Shutdown(SocketShutdown.Send);
                    tcpClient.Close();
                }
                thread.Join();
                ns.Close();
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (SocketException e)
            {
                Console.WriteLine("Server could not be found!");
                Console.WriteLine("Server ip '" + ip_address + "' with port '" + port + "'");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown exception occured!");
                Console.WriteLine(e);
            }
        }
    }
}