using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

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
                byte[] buffer = Encoding.ASCII.GetBytes(s);
                ns.Write(buffer, 0, buffer.Length);
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
                Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }
        }
    }
}