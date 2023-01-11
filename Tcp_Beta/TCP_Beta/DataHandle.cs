using System;
using System.Net.Sockets;
using System.Text;
using CipherOTP;

namespace TCP_Beta
{
    /// <summary>
    /// Class that contains all main function used in TCP Server
    /// </summary>
    public static class DataHandle
    {
        /// <summary>
        /// Object locker for lock
        /// </summary>
        static object locker = new object();

        /// <summary>
        /// List of USER
        /// currently connected users
        /// </summary>
        static List<User> users_list = new List<User>();

        /// <summary>
        /// List of MESSAGE
        /// All messages that were sent in current session
        /// </summary>
        static List<Message> message_list = new List<Message>();

        /// <summary>
        /// Dictionary of STRING, DATETIME
        /// Consists of username and datetime;
        /// 
        /// </summary>
        static Dictionary<string, DateTime> loginHistory = new Dictionary<string, DateTime>();

        static StreamReader sr;
        static StreamWriter sw;

        /// <summary>
        /// Handles client inputed username
        /// Creating and returning user on specified conditions
        /// </summary>
        /// <param name="stream"> Stream of current client </param>
        /// <param name="client"> Current client </param>
        /// <returns> Created user </returns>
        public static User UsernameHandle(NetworkStream stream, TcpClient client)
        {
            try
            {
                string username = String.Empty;
                bool taken = false;
                sw = new StreamWriter(client.GetStream(), Encoding.UTF8);
                while (username == String.Empty)
                {
                    User user = new User(null, client);
                    sw.WriteLine("USERNAME? ~ ");
                    sw.Flush();
                    sr = new StreamReader(stream);
                    username = sr.ReadLine();

                    if (username != null)
                    {
                        username = DecryptMessage(username);
                    }
                    else
                    {
                        sw.WriteLine("!!USERNAME CANNOT BE EMPTY!!");
                        sw.Flush();
                        username = String.Empty;
                    }
                    for (int i = 0; i < users_list.Count; i++)
                    {
                        if (users_list[i].Username == username && taken == false)
                        {
                            sw.WriteLine("!!USER ALREADY EXISTS ON THIS SERVER!!");
                            sw.Flush();
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

        /// <summary>
        /// Handles incoming traffic
        /// checks for unsent messages if user was already on server at least once
        /// proceeds to accept it as command to server or broadcast to all connected users
        /// disconnects user if no message is provided
        /// </summary>
        /// <param name="obj"> User object </param>
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
                    Console.WriteLine(message);

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

        /// <summary>
        /// Broadcast messages with given format
        /// MESSAGE DATE + divider + USERNAME + divider + MESSAGE + new line
        /// </summary>
        /// <param name="username"> Writing user username </param>
        /// <param name="data"> Message with pointer </param>
        /// <param name="currentClient"> Current TCP Client of user </param>
        public static void BroadcastData(string username, string data, User currentClient)
        {
            string message = (DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "];[" + username + "];[" + data + Environment.NewLine);
            lock (locker) message_list.Add(new Message(DateTime.Now, username, data));
            sw = new StreamWriter(currentClient.Client.GetStream(), Encoding.UTF8);
            //locker?
            foreach (User user in users_list)
            {
                if (user != currentClient)
                {
                    sw.WriteLine(message);
                    sw.Flush();
                }
            }

        }

        /// <summary>
        /// Checks for any messages sent within users absence
        /// Sents all unseen messages if user disconnected in between time of his new connection
        /// </summary>
        /// <param name="user"> Reconnected user </param>
        public static void RetrieveHistory(User user)
        {
            if (loginHistory.Any())
            {
                try
                {
                    if (loginHistory.ContainsKey(user.Username))
                    {
                        Console.WriteLine("INVOKE history");
                        DateTime lastLogin = loginHistory[user.Username];
                        for (int i = 0; i < message_list.Count; i++)
                        {
                            int comparison = DateTime.Compare(lastLogin, message_list[i].Time);
                            sw = new StreamWriter(user.Client.GetStream(), Encoding.UTF8);
                            if (comparison < 0)
                            {
                                Console.WriteLine(message_list[i]);
                                string message = (message_list[i].Time + "];[" + message_list[i].Username + "];[" + message_list[i].CurrentMessage + Environment.NewLine);
                                sw.WriteLine(message);
                                sw.Flush();
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Unknown exception occured");
                    Console.WriteLine(e);
                }
            }

        }

        /// <summary>
        /// Disconnects user and informing all sides of connection
        /// </summary>
        /// <param name="currentUser"> User for disconnect </param>
        public static void UserDisconnect(User currentUser)
        {
            lock (locker) users_list.Remove(currentUser);
            sw = new StreamWriter(currentUser.Client.GetStream(), Encoding.UTF8);

            if (loginHistory.ContainsKey(currentUser.Username))
            {
                loginHistory[currentUser.Username] = DateTime.Now;
            }
            else
            {
                loginHistory.Add(currentUser.Username, DateTime.Now);
            }

            Console.WriteLine("User " + currentUser.Username + " disconnected");
            sw.WriteLine("Disconnected by server command");
            sw.Flush();
            ServerWriteALL("!!USER " + currentUser.Username + " DISCONNECTED!!");

            currentUser.Client.Client.Shutdown(SocketShutdown.Both);
            currentUser.Client.Close();
        }

        /// <summary>
        /// Writes message to specified user using his TCP Client
        /// </summary>
        /// <param name="message"> Message to send </param>
        /// <param name="client"> Users TCP Client </param>
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

        /// <summary>
        /// Writes message to all connected users
        /// </summary>
        /// <param name="message"> Message to send </param>
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

        /// <summary>
        /// Accepts message with exclamation mark as first character
        /// Searches for command match in switch 
        /// </summary>
        /// <param name="message"> Command to server </param>
        /// <param name="user"> Current user </param>
        public static void ServerCommands(string message, User user)
        {
            sw = new StreamWriter(user.Client.GetStream(), Encoding.UTF8);
            switch (message)
            {
                case "!quit":
                    UserDisconnect(user);
                    break;
                case "!help":
                    sw.WriteLine("!quit - disconnect from server");
                    sw.Flush();
                    break;
                default:
                    sw.WriteLine("No command found");
                    sw.Flush();
                    break;
            }
        }

        /// <summary>
        /// Decrypts given message with pointer
        /// </summary>
        /// <param name="message"> Message to decrypyt </param>
        /// <returns> Decrypred message </returns>
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
