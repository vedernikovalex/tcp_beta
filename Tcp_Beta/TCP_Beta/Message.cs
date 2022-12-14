using System;
namespace TCP_Beta
{
	public class Message
	{
		private DateTime time;
        private string username;
		private string currentMessage;

        public Message(DateTime time, string username, string currentMessage)//int pointer
        {
            this.Time = time;
            this.Username = username;
            this.CurrentMessage = currentMessage;
        }

        public DateTime Time { get => time; set => time = value; }
        public string Username { get => username; set => username = value; }
        public string CurrentMessage { get => currentMessage; set => currentMessage = value; }

        public override string? ToString()
        {
            return time + " | " + username + " ~ " + currentMessage;
        }
    }
}

