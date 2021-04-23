using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class JellyfinService
    {
        private static readonly int _nbHourActive = 4;
        private static readonly string _ngrokSideApi = "http://localhost:5000";
        private static readonly string _jellyfinPath = @"D:\Apps\JellyFinServer\jellyfin.exe";                
        private List<IMessage> _jellyfinMsg = new List<IMessage>();
        private List<IMessage> _toDelete = new List<IMessage>();

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// If there is no Jellyfin link available return true
        /// CLean aswell the old links
        /// </summary>
        /// <returns></returns>
        internal async Task<bool> IsLinkClear(DiscordSocketClient client)
        {
            IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages;
            var channel = Helper.GetSocketMessageChannel(client, "jellyfin");

            if (channel != null)
            {
                _jellyfinMsg.Clear();
                _toDelete.Clear();

                messages = channel.GetMessagesAsync(50); //recover the last 50 msg
                FillMsgList(messages);
                FillDeleteList();

                if (_toDelete.Count > 0)
                    foreach (var msg in _toDelete) await channel.DeleteMessageAsync(msg);

                if (_jellyfinMsg.Count == 0) // No active link in the channel
                    return true;
            }

            return false;
        }

        internal async Task<string> GetNgrokUrl()
        {            
            string res = await CallSideApiNgrokAsync(_ngrokSideApi);
            return res;                 
        }

        private async Task<string> CallSideApiNgrokAsync(string ngrokPath)
        {
            string jellyfinUrl = "https://www.youtube.com/watch?v=thZ1NiSXh3U";

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(_ngrokSideApi);

            if (response.IsSuccessStatusCode)
                jellyfinUrl = await response.Content.ReadAsStringAsync();

            return jellyfinUrl;
        }

        internal void Activate()
        {
            StartJellyfin(_jellyfinPath);
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
                _jellyfinMsg.AddRange(msg);
            }
        }

        /// <summary>
        /// Select +2hours message about Jellyfin link and remove them from the list
        /// </summary>
        private void FillDeleteList()
        {
            try
            {
                var expiredLinks = _jellyfinMsg.Where(x => _nbHourActive <= (DateTime.Now - x.CreatedAt).TotalHours);
                _toDelete.AddRange(expiredLinks);

                _jellyfinMsg.RemoveAll(x => _nbHourActive <= (DateTime.Now - x.CreatedAt).TotalHours);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void StartJellyfin(string path)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = true
                };

                process.Start();
            }
        }
    }
}