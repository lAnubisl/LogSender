using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.IO;

namespace LogSender
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Production.json", true);
            var configuration = builder.Build();
            var connectionString = configuration["ConnectionStrings"];
            var smtpSettings = new SmtpSettings(
                configuration["SmtpHost"], 
                int.Parse(configuration["SmtpPort"]),
                configuration["SmtpUser"],
                configuration["SmtpPassword"]);
            var to = configuration["To"];
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    var logs = Helper.GetLogs(connection);
                    Helper.SendLogs(to, smtpSettings, logs);
                    Helper.ClearLogs(connection);
                }
                catch (Exception ex)
                {
                    Helper.SendException(to, smtpSettings, ex);
                }
            }
        }
    }
}