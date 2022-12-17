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
        static bool username = false;

        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            List<string> messages = new List<string>();

            try
            {
                tcpClient.Connect(ip_address, port);
                StreamWriter sw = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8);
                Console.WriteLine("Server found!");
                Console.WriteLine("Connected to "+ip_address+ " with port "+port+"!");

                NetworkStream ns = tcpClient.GetStream();

                Thread thread = new Thread(o => Receiver.ReceiveData((TcpClient)o));
                thread.Start(tcpClient);

                string s;
                while (!string.IsNullOrEmpty(s = Console.ReadLine()))
                {
                    try
                    {
                        //TODO -> find a way to simulate network failure
                        if (s == "DROP")
                        {
                            Console.WriteLine("string");
                            NetworkController.Disable();
                        }
                        if (s == ":q")
                        {
                            tcpClient.Client.Shutdown(SocketShutdown.Send);
                            thread.Join();
                            ns.Close();
                            tcpClient.Close();
                            Console.ReadKey();
                        }

                        if (!username)
                        {
                            sw.WriteLine(s);
                            sw.Flush();
                            username = true;
                            //todo WRONG -> make encryption on serverside
                        }

                        (string message, int pointer) = Crypt.EncryptXOR(s);
                        messages.Append(message + "];[" + pointer);

                        sw.WriteLine(message + "];[" + pointer);
                        sw.Flush();
                        messages.Clear();

                        //byte[] buffer = Encoding.ASCII.GetBytes(message + "];[" + pointer);
                        //ns.Write(buffer, 0, buffer.Length);
  
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        Console.WriteLine(e);
                    }

                }
                Console.WriteLine("disconnected");
                tcpClient.Client.Shutdown(SocketShutdown.Send);
                thread.Join();
                ns.Close();
                tcpClient.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("server could not be found!");
                //TODO actions
            }
        }
    }
}