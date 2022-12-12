﻿using System;
using System.Net.Sockets;

namespace TCP_Beta
{
	public class User
	{
		private string username;
		private TcpClient client;

        public User(string username, TcpClient client)
        {
            this.Username = username;
            this.Client = client;
        }

        public string Username { get => username; set => username = value; }
        public TcpClient Client { get => client; set => client = value; }

        public override string? ToString()
        {
            return username;
        }
    }
}

