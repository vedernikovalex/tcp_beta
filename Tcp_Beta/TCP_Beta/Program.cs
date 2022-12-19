using System;
using System.Net;
using System.Net.Sockets;

namespace TCP_Beta
{
    class Program
    {
        /// <summary>
        /// Boolean for server activity
        /// </summary>
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
                    if (currentUser == null)
                    {
                        currentUser = DataHandle.UsernameHandle(stream, client);
                    }

                    if (currentUser != null)
                    {
                        Console.WriteLine("User " + currentUser.Username + " just connected!");
                        DataHandle.ServerWrite("!! Type '!help' for server commands !!",currentUser.Client);
                        DataHandle.ServerWrite(" ", currentUser.Client);
                        DataHandle.ServerWriteALL("User " + currentUser.Username + " just connected!\n");
                        Thread thread = new Thread(DataHandle.HandleData);
                        //Thread timeout = new Thread(TimeOut);
                        thread.Start(currentUser);
                    }
                }
                catch (IOException e)
                {
                    if (currentUser != null)
                    {
                        DataHandle.UserDisconnect(currentUser);
                    }
                    Console.WriteLine("User '" + currentUser.Username + "' aborted connection by hard shutdown!");
                    Console.WriteLine("Error: " + e.Message + "\n");
                    DataHandle.UserDisconnect(currentUser);
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("Error occured in username input!");
                    Console.WriteLine("Error: " + e.Message + "\n");
                    DataHandle.UserDisconnect(currentUser);
                }
            }
        }

    }
}