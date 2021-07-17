using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class JellyfinService
    {
        private static readonly int _nbHourActive = 14;
        private static readonly string _ngrokSideApi = "http://localhost:5000";
        private static readonly string _jellyfinPath = @"D:\Apps\JellyFinServer\jellyfin.exe";                
        private static readonly string _ngrokSideApiPath = @"C:\Users\vgusm\Desktop\v1\ApiNgrok\Ngrok.AspNetCore.Sample.exe";
        private List<IMessage> _jellyfinMsg = new List<IMessage>();
        private List<IMessage> _toDelete = new List<IMessage>();

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


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
                _jellyfinMsg.Clear();
                _toDelete.Clear();

                messages = channel.GetMessagesAsync(50); //recover the last 50 msg
                FillMsgList(messages);
                FillDeleteList();

                if (_toDelete.Count > 0)
                    foreach (var msg in _toDelete) await channel.DeleteMessageAsync(msg);
            }
        }

        internal async Task<string> GetNgrokUrl()
        {
            if (!Process.GetProcessesByName("ngrok").Any())
            {
                StartNgrokSideApi();
                Thread.Sleep(10000); // wait 10sec
            }                         
                        
            string res = await CallSideApiNgrokAsync(_ngrokSideApi);
            return res;                 
        }

        private async Task<string> CallSideApiNgrokAsync(string ngrokPath)
        {
            string jellyfinUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(_ngrokSideApi);

            if (response.IsSuccessStatusCode)
                jellyfinUrl = await response.Content.ReadAsStringAsync();

            return jellyfinUrl;
        }

        /// <summary>
        /// Check if there is any process already running, then start Jellyfin
        /// </summary>
        internal void Activate()
        {
            if (!Process.GetProcessesByName("jellyfin").Any())
            {
                StartExe(_jellyfinPath);
                Thread.Sleep(5000); // wait 5sec
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
                _jellyfinMsg.AddRange(msg);
                _jellyfinMsg.AddRange(msg2);
            }
        }

        /// <summary>
        /// Select +xhours message about Jellyfin link and remove them from the list
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

        internal Task RestartSideApi()
        {
            foreach (var p in Process.GetProcessesByName("chrome")) //for RAM lmao
            {
                p.Kill();
            }
            foreach (var p in Process.GetProcessesByName("ngrok"))
            {
                p.Kill();
            }
            foreach (var p in Process.GetProcessesByName("Ngrok.AspNetCore.Sample"))
            {
                p.Kill();
            }

            StartNgrokSideApi();

            return Task.CompletedTask;
        }

        private void StartNgrokSideApi()
        {
            StartExe(_ngrokSideApiPath);
        }

        private void StartExe(string path)
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