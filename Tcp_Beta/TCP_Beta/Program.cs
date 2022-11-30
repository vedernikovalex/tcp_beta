using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCP_Beta
{
    class Program
    {
        static Dictionary<int, TcpClient> client_list = new Dictionary<int, TcpClient>();
        static object locker = new object();

        static void Main(string[] args)
        {
            int count = 1;

            TcpListener server = new TcpListener(IPAddress.Any, 7777);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                lock (locker) client_list.Add(count, client);
                Console.WriteLine("Connection established!");

                Thread thread = new Thread(handle);
                thread.Start(count);
                count++;
            }

        }

        public static void handle(object obj)
        {
            int id = (int)obj;
            TcpClient client;
            lock (locker) client = client_list[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(data);
                Console.WriteLine(data);
            }

            lock (locker) client_list.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void broadcast(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);
            lock (locker)
            {
                foreach(TcpClient client in client_list.Values)
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}