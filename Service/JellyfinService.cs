using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class JellyfinService
    {
        private static readonly string _jellyfinPath = @"D:\Apps\JellyFinServer\jellyfin.exe";        
        private static readonly string _ngrokArgs = "help";
        //private static readonly string _ngrokArgs = "ngrok http -region=eu 8096";
        private static readonly string _ngrokPath = @"D:\Apps\Ngrok\ngrok.exe";
        //private static readonly string _ngrokPath = @"D:\Apps\Ngrok\jellyfin.bat";
        private List<IMessage> _jellyfinMsg = new List<IMessage>();        
        private List<IMessage> _toDelete = new List<IMessage>();

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //

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

        internal async Task<string> OpenIP()
        {
            string ngrokUrl = StartProcessAsync(_ngrokPath, true);
            
            // "http://xxxxx.eu.ngrok.io";

            return ngrokUrl;
        }

        internal Task Activate()
        {
            //StartProcess(_jellyfinPath, true);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Select message about Jellyfin link
        /// </summary>
        /// <param name="messages"></param>
        private void FillMsgList(IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages)
        {                        
            var msgAsync = messages.ToListAsync().Result;
            List<IEmbed> embeds = new List<IEmbed>();

            foreach(var list in msgAsync)
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
            var expiredLinks = _jellyfinMsg.Where(x => 2 <= (DateTime.Now - x.CreatedAt).TotalHours);
            _toDelete.AddRange(expiredLinks);

            foreach (var expired in expiredLinks) { _jellyfinMsg.Remove(expired); }
        }


        private string StartProcessAsync(string path, bool newWindow)
        {
            string resUrl = null;
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path,                        
                        CreateNoWindow = newWindow,
                        WindowStyle = ProcessWindowStyle.Normal,
                        UseShellExecute = false,
                        //UseShellExecute--> 
                        RedirectStandardOutput = true, 
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
//                      Arguments = _ngrokArgs
                    }
                };

                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null) return;

                    Console.WriteLine(e.Data);
                };


                //process.OutputDataReceived += new DataReceivedEventHandler();                
                process.Start();

                string output = "";

                //using (Stream stream = new MemoryStream())
                //{
                //    using (StreamReader streamReader = process.StandardOutput)
                //    {
                //        //Console.WriteLine(await streamReader.ReadToEndAsync());                        
                //        //input
                //        //StreamWriter writer = process.StandardInput;
                //        


                //    }
                //}          
                process.StandardInput.WriteLine(_ngrokArgs);
                //using (StreamWriter writer = process.StandardInput)
                //{
                //    writer.WriteLine(_ngrokArgs);
                //    await writer.WriteAsync(_ngrokArgs);
                //}

                //extract url from output
                output = process.StandardOutput.ReadToEnd();
                string outpute = process.StandardError.ReadToEnd();
                Console.WriteLine(output);

                resUrl = output;
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
            return "";
        }

        //private DataReceivedEventHandler Oue()
        //{
            
        //}
    }
}
