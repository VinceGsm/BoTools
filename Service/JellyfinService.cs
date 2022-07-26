using Discord;
using Discord.WebSocket;
using NgrokApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class JellyfinService
    {                                
        private static readonly string _ngrokBatPath = @"C:\Program Files\Ngrok\ngrok.bat";
        private static readonly string _jellyfinPath = @"C:\Program Files\Jellyfin\jellyfin_10.7.7\jellyfin.exe";
        private List<IMessage> _toDelete = new List<IMessage>();        


        /// <summary>        
        /// Clean the old links
        /// </summary>
        /// <returns></returns>
        internal async Task ClearChannel(DiscordSocketClient client)
        {
            IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages;
            var channel = Helper.GetSocketMessageChannel(client, 816283362478129182); //Jellyfin

            if (channel != null)
            {                
                _toDelete.Clear();

                messages = channel.GetMessagesAsync(50); //recover the last 50 msg                
                FillMsgList(messages);                

                if (_toDelete.Count > 0)
                    foreach (var msg in _toDelete) await channel.DeleteMessageAsync(msg);
            }
        }

        internal async Task<string> GetNgrokUrl()
        {
            if (!Process.GetProcessesByName("ngrok").Any())
            {
                Helper.StartProcess(_ngrokBatPath);
                Thread.Sleep(1000); // wait 1sec
            }

            string res = await GetJellyfinUrl();
            return res;                 
        }

        private async Task<string> GetJellyfinUrl()
        {            
            var ngrok = new Ngrok(Environment.GetEnvironmentVariable("NGROK_API_KEY"));

            Tunnel jellyfinTunnel = await ngrok.Tunnels.List().FirstAsync();
            return jellyfinTunnel.PublicUrl;
        }

        /// <summary>
        /// Check if there is any process already running, then start Jellyfin
        /// </summary>
        internal void Activate()
        {
            if (!Process.GetProcessesByName("jellyfin").Any())
            {
                Helper.StartProcess(_jellyfinPath);
                Thread.Sleep(4000); // wait 4sec
            }
        }


        /// <summary>
        /// Select message about Jellyfin link
        /// </summary>
        /// <param name="messages"></param>
        private void FillMsgList(IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages)
        {
            var msgAsync = messages.ToListAsync().Result;

            foreach (var list in msgAsync)
            {
                IEnumerable<IMessage> msg = list.Where(x => x.Content.StartsWith("<a:pepeSmoke:830799658354737178>"));
                IEnumerable<IMessage> msg2 = list.Where(x => x.Content.StartsWith("<a:luffy:863101041498259457>"));
                _toDelete.AddRange(msg);
                _toDelete.AddRange(msg2);
            }
        }
    }
}