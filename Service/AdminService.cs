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
            _client.Ready += Ready;
            _client.LeftGuild += LeftGuild;
        }


        /// <summary>
        /// When guild data has finished downloading (+state : Ready)
        /// </summary>
        /// <returns></returns>
        public Task Ready()
        {
            // msg latency right after the connection //_client.Latency
            // method check taille dossier log : si > x --> supprimer 2 +vieux fichier de log       ////////////////////////////////////////////////////////////////
            // method qui va supp dernier message si +2h jellyfin
            // 1x/ jour recup liste ID (nécessaire ?)

            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task LeftGuild(SocketGuild arg)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task Log(LogMessage msg)
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
            return Task.CompletedTask;
        }
    }
}
