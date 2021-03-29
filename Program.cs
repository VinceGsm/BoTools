using Discord;
using Discord.WebSocket;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BoTools
{
	public class Program
	{
		private DiscordSocketClient _client;
		private string _token = Environment.GetEnvironmentVariable("BoTools_Token");
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void Main(string[] args) 
			=> new Program().MainAsync().GetAwaiter().GetResult();


		public async Task MainAsync()
        {
            LoadLogConfig();

            _client = new DiscordSocketClient(); 
            //TODO : msg latency    
			_client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }


        private static void LoadLogConfig()
        {            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        private Task Log(LogMessage msg)
		{
            //TODO : service ?
            
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
                    log.Info(msg.Message);
                    break;
            }			
			return Task.CompletedTask;
		}
	}
}
