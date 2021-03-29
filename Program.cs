using Discord;
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
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void Main(string[] args) 
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
        {
            LoadLogConfig();

        }

        private static void LoadLogConfig()
        {            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        private Task Log(LogMessage msg)
		{
			log.Info(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
