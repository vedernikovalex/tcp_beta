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
        static StreamReader sr;
        static StreamWriter sw;
        static bool isRunning;

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 7777);
            server.Start();
            isRunning = true;
            Console.WriteLine("Server is running");

            while (isRunning)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                User currentUser = null;

                try
                {
                    /*
                    Thread usernameThread = new Thread(() => { currentUser = UsernameHandle(stream, client); });
                    usernameThread.Start();
                    usernameThread.Join();
                    */

                    currentUser = UsernameHandle(stream, client);

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
                    if (currentUser != null)
                    {
                        lock (locker) users_list.Remove(currentUser);
                    }
                    Console.WriteLine("User '" + currentUser.Username + "' aborted connection by hard shutdown!");
                    Console.WriteLine("Error: " + e.Message + "\n");
                    UserDisconnect(currentUser);
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("Error occured in username input!");
                    Console.WriteLine("Error: " + e.Message + "\n");
                    UserDisconnect(currentUser);
                }
            }
        }

        public static User UsernameHandle(NetworkStream stream, TcpClient client)
        {
            try
            {
                string username = String.Empty;
                bool taken = false;
                while (username == String.Empty)
                {
                    ServerWrite("USERNAME? ~ ", client);
                    sr = new StreamReader(stream);
                    username = sr.ReadLine();

                    if (username == null)
                    {
                        ServerWrite("!!USERNAME CANNOT BE EMPTY!!\n", client);
                        username = String.Empty;
                    }
                    else
                    {
                        //string input = Encoding.ASCII.GetString(receiveAuthorize, 0, byte_count);
                        for (int i = 0; i < users_list.Count; i++)
                        {
                            if (users_list[i].Username == username && taken == false)
                            {
                                ServerWrite("!!USER ALREADY EXISTS ON THIS SERVER!!\n", client);
                                taken = true;
                                username = String.Empty;
                            }
                        }
                    }
                    if (taken == false && username != String.Empty)
                    {
                        User currentUser = new User(username, client);
                        lock (locker) users_list.Add(currentUser);
                        return currentUser;
                    }
                }
                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown user aborted connection by hard shutdown!");
                Console.WriteLine("Error: "+e.Message + "\n");
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
                    //TODO handle all users disconnectance
                    string message = sr.ReadLine();

                    if(message != null)
                    {
                        if (message[0] == '!')
                        {
                            ServerCommands(message, currentUser);
                        }
                        else
                        {
                            broadcast(currentUser.Username, message, currentUser);
                        }
                    }
                    else
                    {
                        UserDisconnect(currentUser);
                    }
                }
                
            }
            catch (IOException e)
            {
                Console.WriteLine("User '"+ currentUser + "' aborted connection by hard shutdown!");
                Console.WriteLine("Error: " + e.Message+"\n");
                if (currentUser != null)
                {
                    UserDisconnect(currentUser);
                }
            }
        }
        /*
        public static void TimeOut(object obj)
        {

        }
        */
        public static void broadcast(string username, string data, User currentClient)
        {
            string message = (DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "];[" + username + "];[" + data + Environment.NewLine);
            lock (locker) message_list.Add(new Message(DateTime.Now, username, data));
            
            //locker?
            foreach (User user in users_list)
            {
                if (user != currentClient)
                {
                    ServerWrite(message, user.Client);
                }
            }
            
        }

        public static void RetrieveHistory(User user)
        {
            if (loginHistory.ContainsKey(user.Username))
            {
                Console.WriteLine("INVOKE history");
                DateTime lastLogin = loginHistory[user.Username];
                for (int i = 0; i < message_list.Count; i++)
                {
                    int comparison = DateTime.Compare(lastLogin, message_list[i].Time);
                    if (comparison < 0)
                    {
                        Console.WriteLine(message_list[i]);
                        string message = (message_list[i].Time + "];[" + message_list[i].Username + "];[" + message_list[i].CurrentMessage + Environment.NewLine);
                        ServerWrite(message, user.Client);
                    }
                }
            }
        }

        public static void UserDisconnect(User currentUser)
        {
            lock (locker) users_list.Remove(currentUser);

            if (loginHistory.ContainsKey(currentUser.Username))
            {
                loginHistory[currentUser.Username] = DateTime.Now;
            }
            else
            {
                loginHistory.Add(currentUser.Username, DateTime.Now);
            }

            Console.WriteLine("User " + currentUser.Username + " disconnected");
            ServerWriteALL("!!USER " + currentUser.Username + " DISCONNECTED!!");

            currentUser.Client.Client.Shutdown(SocketShutdown.Both);
            currentUser.Client.Close();
        }


        public static void ServerWrite(string message, TcpClient client)
        {
            sw = new StreamWriter(client.GetStream(), Encoding.UTF8);
            try
            {
                sw.WriteLine(message + Environment.NewLine);
                sw.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("No active stream found!");
                Console.WriteLine(e.Message);
            }

        }

        public static void ServerWriteALL(string message)
        {
            try
            {
                foreach (User user in users_list)
                {
                    sw = new StreamWriter(user.Client.GetStream(), Encoding.UTF8);
                    sw.WriteLine(message + Environment.NewLine);
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("No active stream found!");
                Console.WriteLine(e.Message);
            }
        }

        public static void ServerCommands(string message, User user)
        {
            Console.WriteLine("invoke method");
            switch (message)
            {
                case "!quit":
                    Console.WriteLine("Invoke");
                    UserDisconnect(user);
                    break;

                default:
                    ServerWrite("No command found", user.Client);
                    break;
            }
        }
    }
}