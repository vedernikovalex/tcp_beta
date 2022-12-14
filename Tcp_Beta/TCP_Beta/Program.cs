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
                try
                {
                    Thread usernameThread = new Thread(() => { currentUser = UsernameHandle(stream, client); });
                    usernameThread.Start();
                    usernameThread.Join();
                    if (currentUser != null)
                    {
                        Console.WriteLine("User " + currentUser.Username + " just connected!");
                        Thread thread = new Thread(Handle);
                        //Thread timeout = new Thread(TimeOut);
                        thread.Start(currentUser);
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("User aborted connection by hard shutdown!");
                    Console.WriteLine("Error: "+e.Message + "\n");
                    if(currentUser != null)
                    {
                        lock (locker) users_list.Remove(currentUser);
                    }
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("Error occured in username input!");
                    Console.WriteLine("Error: " + e.Message + "\n");
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
        }

        public static User UsernameHandle(NetworkStream stream, TcpClient client)
        {
            try
            {
                //TODO -> ask username for every connected USER
                //          handle in thread??
                //          dead end..
                string username = String.Empty;
                while (username == String.Empty)
                {
                    bool taken = false;
                    ServerWrite("USERNAME? ~ ", stream);
                    byte[] receiveAuthorize = new byte[1024];
                    int byte_count = stream.Read(receiveAuthorize, 0, receiveAuthorize.Length);
                    if (byte_count <= 0)
                    {
                        ServerWrite("!!USERNAME CANNOT BE EMPTY!!\n", stream);
                        //stream.Position = 0;
                    }
                    else
                    {
                        string input = Encoding.ASCII.GetString(receiveAuthorize, 0, byte_count);
                        for (int i = 0; i < users_list.Count; i++)
                        {
                            if (users_list[i].Username == input)
                            {
                                ServerWrite("!!USER ALREADY EXISTS ON THIS SERVER!!\n", stream);
                                taken = true;
                            }
                        }
                        if (!taken)
                        {
                            username = input;
                        }
                    }
                }
                //TODO handle empty username assign
                User currentUser = new User(username, client);
                lock (locker) users_list.Add(currentUser);
                return currentUser;
            }
            catch (IOException e)
            {
                Console.WriteLine("User aborted connection by hard shutdown!");
                Console.WriteLine("Error: "+e.Message + "\n");
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
                return null;
            }
        }

        public static void Handle(object obj)
        {
            User currentUser = (User)obj;
            TcpClient client = currentUser.Client;
            NetworkStream stream = client.GetStream();
            try
            {
                RetrieveHistory(currentUser);

                while (client.Connected)
                {
                    byte[] buffer = new byte[1024];
                    //TODO handle all users disconnectance
                    int byte_count = stream.Read(buffer, 0, buffer.Length);

                    if (byte_count == 0)
                    {
                        break;
                    }

                    string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                    broadcast(currentUser.Username, data, currentUser);
                }
                lock (locker) users_list.Remove(currentUser);

                if (loginHistory.ContainsKey(currentUser.Username))
                {
                    loginHistory[currentUser.Username] = DateTime.Now;
                }
                loginHistory.Add(currentUser.Username, DateTime.Now);

                Console.WriteLine("User "+ currentUser.Username+" disconnected");
                ServerWrite("!!USER "+ currentUser.Username+" DISCONNECTED!!", stream);

                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("User '"+ currentUser + "' aborted connection by hard shutdown!");
                Console.WriteLine("Error: " + e.Message+"\n");
                if (currentUser != null)
                {
                    lock (locker) users_list.Remove(currentUser);
                }
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
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
            try
            {
                byte[] serverByte = Encoding.UTF8.GetBytes(message);
                stream.Write(serverByte);
            }
            catch (IOException e)
            {
                Console.WriteLine("No active stream found!");
                Console.WriteLine(e.Message);
            }
        }

        public static void ServerWriteALL(string message, NetworkStream stream)
        {
            //TODO -> write to all users a message
        }

        public static void ServerCommands()
        {
            //TODO -> accept user commands to handle on server side
        }
    }
}