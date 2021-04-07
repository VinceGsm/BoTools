using log4net;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class JellyfinService
    {
        private static readonly string _jellyfinPath = "";
        private static readonly string _ngrokPath = "";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //P1 :  [text] (link)


        /// <summary>
        /// Clean all past msg about Jellyfin if necessary
        /// </summary>
        /// <returns></returns>
        public async Task Clean()
        {

            return;
        }
    }
}
