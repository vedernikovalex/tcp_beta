using System;
using System.Net.Sockets;
using System.Text;

namespace TCP_Beta
{
    /// <summary>
    /// Class of USER; contains string users username and users TcpClient client connection;
    /// </summary>
	public class User
	{
		private string username;
		private TcpClient client;
        private StreamWriter sw;

        public User(string username, TcpClient client)
        {
            this.Username = username;
            this.Client = client;
            sw = new StreamWriter(client.GetStream(), Encoding.UTF8);
        }

        public string Username { get => username; set => username = value; }
        public TcpClient Client { get => client; set => client = value; }

        public void Print(string message)
        {
            sw.WriteLine(message);
            sw.Flush();
        }

        public override string? ToString()
        {
            return username;
        }
    }
}

