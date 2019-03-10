using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace LogSender
{
    internal static class Helper
    {
        internal static string ToString(IEnumerable<LogEntry> logs)
        {
            var sb = new StringBuilder();
            foreach(var log in logs)
            {
                sb.AppendLine($"{log.Logged} - {log.Level} - {log.Application} - {log.Logger} - {log.Message} - {log.Exception}");
            }

            return sb.ToString();
        }

        internal static byte[] Zip(string textToZip)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var file = zipArchive.CreateEntry("logs.txt");
                    using (var entryStream = file.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            streamWriter.Write(textToZip);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        internal static void ClearLogs(IDbConnection connection)
        {
            connection.Execute("DELETE FROM log");
        }

        internal static IEnumerable<LogEntry> GetLogs(IDbConnection connection)
        {
            return connection.Query<LogEntry>("SELECT * FROM log");
        }

        internal static string GetMessageBody(IEnumerable<LogEntry> logs)
        {
            var groups = logs.GroupBy(l => l.Level);
            var sb = new StringBuilder();
            foreach (var group in groups)
            {
                sb.AppendLine($"Level '{group.Key}'. {group.Count()} lines.");
            }

            return sb.ToString();
        }

        internal static void SendLogs(string to, SmtpSettings settings, IEnumerable<LogEntry> logs)
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress("admin@petproject.by");
                message.To.Add(new MailAddress(to));
                message.Subject = $"Logs for {DateTime.Now.ToString("dd/MM/yyyy")}";
                message.Body = GetMessageBody(logs);
                message.Attachments.Add(new Attachment(new MemoryStream(Zip(ToString(logs))), "logs.zip", "application/zip"));
                Send(message, settings);
            }  
        }

        internal static void SendException(string to, SmtpSettings settings, Exception ex)
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress("admin@petproject.by");
                message.To.Add(new MailAddress(to));
                message.Subject = $"Exception in LogSender";
                message.Body = ex.Message + Environment.NewLine + ex.StackTrace;
                Send(message, settings);
            }
        }

        private static void Send(MailMessage message, SmtpSettings settings)
        {
            using (var client = new SmtpClient(settings.Host, settings.Port))
            {
                client.Credentials = new NetworkCredential(settings.User, settings.Password);
                client.Send(message);
            }
        }
    }
}