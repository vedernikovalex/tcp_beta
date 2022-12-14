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
        static object locker = new object();

        static List<User> users_list = new List<User>();
        static List<Message> message_list = new List<Message>();
        static Dictionary<string, DateTime> loginHistory = new Dictionary<string, DateTime>();
        
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 7777);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                User currentUser = null;
                while (currentUser == null)
                {
                    currentUser = UsernameHandle(stream, client);
                }
                Console.WriteLine("User "+ currentUser.Username + " just connected!");

                Thread thread = new Thread(Handle);
                //Thread timeout = new Thread(TimeOut);
                thread.Start(currentUser);
            }
        }

        public static User UsernameHandle(NetworkStream stream, TcpClient client)
        {
            //TODO unique username to user, only one username can be logged in
            //TODO ??? missing -> if working = fine
            string username = String.Empty;
            while (username == String.Empty)
            {
                ServerWrite("USERNAME? ~ ", stream);
                byte[] receiveAuthorize = new byte[1024];
                int byte_count = stream.Read(receiveAuthorize, 0, receiveAuthorize.Length);
                if (byte_count <= 0)
                {
                    ServerWrite("!!USERNAME CANNOT BE EMPTY!! ~ ", stream);
                    stream.Position = 0;
                }
                else
                {
                    string input = Encoding.ASCII.GetString(receiveAuthorize, 0, byte_count);
                    username = input;
                }
            }
            //TODO handle empty username assign
            User currentUser = new User(username, client);
            lock (locker) users_list.Add(currentUser);
            return currentUser;
        }

        public static void Handle(object obj)
        {
            User user = (User)obj;
            TcpClient client = user.Client;

            RetrieveHistory(user);

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                //TODO handle all users disconnectance
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(user.Username, data, user);
            }
            lock (locker) users_list.Remove(user);

            if (loginHistory.ContainsKey(user.Username))
            {
                loginHistory[user.Username] = DateTime.Now;
            }
            loginHistory.Add(user.Username, DateTime.Now);

            //TODO print user disconnected to both client and server

            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        /*
        public static void TimeOut(object obj)
        {

        }
        */
        public static void broadcast(string username, string data, User currentClient)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "];["+username+"];["+data + Environment.NewLine);
            lock (locker) message_list.Add(new Message(DateTime.Now, username, data));
            lock (locker)
            {
                foreach (User user in users_list)
                {
                    if (user != currentClient)
                    {
                        NetworkStream stream = user.Client.GetStream();
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        public static void RetrieveHistory(User user)
        {
            if (loginHistory.ContainsKey(user.Username))
            {
                DateTime lastLogin = loginHistory[user.Username];
                NetworkStream stream = user.Client.GetStream();
                for (int i = 0; i < message_list.Count; i++)
                {
                    int comparison = DateTime.Compare(lastLogin, message_list[i].Time);
                    if (comparison < 0)
                    {
                        Console.WriteLine(message_list[i]);
                        byte[] buffer = Encoding.ASCII.GetBytes(message_list[i].Time + "];[" + message_list[i].Username + "];[" + message_list[i].CurrentMessage + Environment.NewLine);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        public static void ServerWrite(string message, NetworkStream stream)
        {
            byte[] serverByte = Encoding.UTF8.GetBytes(message);
            stream.Write(serverByte);
        }

        public static void ServerCommands()
        {
            //TODO accept user commands to handle on server side
        }
    }
}