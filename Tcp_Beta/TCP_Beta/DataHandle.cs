using System;
using System.Net.Sockets;
using System.Text;
using CipherOTP;

namespace TCP_Beta
{
    public static class DataHandle
    {
        static object locker = new object();

        static List<User> users_list = new List<User>();

        static List<Message> message_list = new List<Message>();
        static Dictionary<string, DateTime> loginHistory = new Dictionary<string, DateTime>();

        static StreamReader sr;
        static StreamWriter sw;

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

                    if (username != null)
                    {
                        username = DecryptMessage(username);
                    }
                    else
                    {
                        ServerWrite("!!USERNAME CANNOT BE EMPTY!!", client);
                        username = String.Empty;
                    }
                    for (int i = 0; i < users_list.Count; i++)
                    {
                        if (users_list[i].Username == username && taken == false)
                        {
                            ServerWrite("!!USER ALREADY EXISTS ON THIS SERVER!!", client);
                            taken = true;
                            username = String.Empty;
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
                Console.WriteLine("Error: " + e.Message + "\n");
                client.Close();
                return null;
            }
        }


        public static void HandleData(object obj)
        {
            User currentUser = (User)obj;
            TcpClient client = currentUser.Client;
            try
            {
                RetrieveHistory(currentUser);

                while (client.Connected)
                {
                    sr = new StreamReader(client.GetStream());
                    string message = sr.ReadLine();

                    if (message != null)
                    {
                        string decryptedMsg = DecryptMessage(message);
                        if (decryptedMsg[0] == '!')
                        {
                            ServerCommands(decryptedMsg, currentUser);
                        }
                        else
                        {
                            BroadcastData(currentUser.Username, message, currentUser);
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
                Console.WriteLine("User '" + currentUser + "' aborted connection by hard shutdown!");
                Console.WriteLine("Error: " + e.Message + "\n");
                if (currentUser != null)
                {
                    UserDisconnect(currentUser);
                }
            }
        }


        public static void BroadcastData(string username, string data, User currentClient)
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
            ServerWrite("Disconnected by server command", currentUser.Client);
            ServerWriteALL("!!USER " + currentUser.Username + " DISCONNECTED!!");

            currentUser.Client.Client.Shutdown(SocketShutdown.Both);
            currentUser.Client.Close();
        }


        public static void ServerWrite(string message, TcpClient client)
        {
            sw = new StreamWriter(client.GetStream(), Encoding.ASCII);
            try
            {
                sw.WriteLine(message + Environment.NewLine + "\n");
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
                    sw = new StreamWriter(user.Client.GetStream(), Encoding.ASCII);
                    sw.WriteLine(message + Environment.NewLine +"\n");
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
            switch (message)
            {
                case "!quit":
                    UserDisconnect(user);
                    break;

                default:
                    ServerWrite("No command found", user.Client);
                    break;
            }
        }

        public static string DecryptMessage(string message)
        {
            string[] message_key = message.Split("];[");
            try
            {
                return Crypt.DecryptXOR(message_key[0], Int32.Parse(message_key[1]));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured");
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
