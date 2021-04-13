using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class AdminService
    {
        private DiscordSocketClient _client;        
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public AdminService(DiscordSocketClient client)
        {
            _client = client;            
            _client.Log += Log;
        }


        public Task Log(LogMessage msg)
        {
            if(msg.Exception == null)
            {
                switch (msg.Severity)
                {
                    case LogSeverity.Critical:
                    case LogSeverity.Error:
                        log.Error(msg.Message);
                        break;

                    case LogSeverity.Warning:
                        log.Warn(msg.Message);
                        break;

                    case LogSeverity.Verbose:
                        log.Debug(msg.Message);
                        break;

                    default:
                        Console.WriteLine($"{DateTime.Now} : {msg.Message}");
                        break;
                }
            }
            else
                log.Error(msg.Exception.Message);

            return Task.CompletedTask;
        }
    }
}
