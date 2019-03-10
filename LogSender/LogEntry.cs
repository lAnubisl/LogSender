using System;

namespace LogSender
{
    public class LogEntry
    {
        public string Application { get; set; }
        public DateTime Logged { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Logger { get; set; }
        public string Exception { get; set; }
        public string ActivityId { get; set; }
    }
}