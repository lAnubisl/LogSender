namespace LogSender
{
    public class SmtpSettings
    {
        public SmtpSettings(string host, int port, string user, string password)
        {
            Host = host;
            Port = port;
            User = user;
            Password = password;
        }

        public string Host { get; }
        public int Port { get; }
        public string User { get; }
        public string Password { get; }
    }
}