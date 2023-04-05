using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class RoleService
    {        
        private static ulong _readTheRulesId = 847048535799234560;
        private static ulong _gamingDealId = 1092072288226115685;

        private bool _connexion = true;
        private IRole _IRoleRules = null;                 
        private List<IRole> _IRolesSeparators = new List<IRole>();        
        List<SocketGuildUser> _allUsers = new List<SocketGuildUser>();

        public static readonly List<ulong> _roleSeparatorIds = new List<ulong>
        {
            1061919166199775232, //_separatorBonusId
            1061919390607605770, //_separatoIrlId
            1052542257737256980, //_separatorPrivilegesId
            1052521533135917137, //_separatorAccreditationsId                        
        };

        private DiscordSocketClient _client;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public RoleService(DiscordSocketClient client)
        {
            _client = client;
            _client.GuildMembersDownloaded += GuildMembersDownloaded;            
        }

        
        private async Task GuildMembersDownloaded(SocketGuild arg)
        {
            log.Info($"| GuildMembersDownloaded IN --> firstIN={_connexion}");
            if (_connexion)
            {                
                await CheckRoles();
                await NotifRoles();
                _connexion = false;
            }
            log.Info("| GuildMembersDownloaded out");
        }

        private async Task NotifRoles()
        {
            NotifGamingDeal();
        }

        private async void NotifGamingDeal()
        {
            #region Jeudi = Free Epic Store
            if (Helper.IsThursdayToday())
            {
                ISocketMessageChannel mediaChannel = Helper.GetSocketMessageChannel(_client, 494958624922271745);

                List<IReadOnlyCollection<IMessage>> lastMsgAsync = await mediaChannel.GetMessagesAsync(20).ToListAsync();
                bool isNew = true;

                foreach (var msg in lastMsgAsync)
                {
                    foreach (var wtfDiscord in msg)
                    {
                        if (wtfDiscord.CreatedAt.Day == DateTime.Today.Day || wtfDiscord.Author.IsBot)
                            isNew = false;                        
                    }
                }                

                if (isNew)
                {
                    string message = $"{Helper.GetPikachuEmote()} <@&{_gamingDealId}> \n" +
                        $"C'est Jeudi_EPI ! n'oublier pas de recup le ou les jeux gratuit sur le Store EPIC GAMES";

                    if (mediaChannel != null)
                        await mediaChannel.SendMessageAsync(message, isTTS: true);
                }
            }
            #endregion
        }

        public async Task CheckRoles()
        {
            log.Info($"CheckRoles IN");
            if (_IRoleRules == null) _IRoleRules = Helper.GetRoleById(_client, _readTheRulesId);

            if (_allUsers.Count == 0)
            {
                _allUsers = Helper.GetZderLand(_client).Users.ToList();
                _allUsers.RemoveAll(x => x.IsBot);
            }

            await CheckRules();

            if (_IRolesSeparators.Count == 0)
                FillRoles();            

            log.Info($"CheckRoles OUT");
        }

        private void FillRoles()
        {
            _IRolesSeparators = Helper.GetIRolesFromServer(_client, _roleSeparatorIds).ToList();           
        }

        private async Task CheckRules()
        {
            log.Info($"CheckRules IN");

            var chrono = new Stopwatch();
            chrono.Start();

            var channelRules = Helper.GetSocketMessageChannel(_client, 846694705177165864); //rôles
            IReadOnlyCollection<IMessage> iMsg = channelRules.GetMessagesAsync(1).FirstAsync().Result;
            IMessage msg = iMsg.First();

            List<IReadOnlyCollection<IUser>> reactListUsers = msg.GetReactionUsersAsync(msg.Reactions.FirstOrDefault().Key, 1000).ToListAsync().Result;

            foreach (var userLst in reactListUsers)
            {
                var okUserslist = userLst.ToList();

                foreach (var okUser in okUserslist)
                {
                    if (okUser.Id != 493020872303443969) // compte qui met les reaction 
                    {
                        var subject = _allUsers.First(x => x.Id == okUser.Id);                        
                        
                        if (!subject.Roles.Contains(_IRoleRules))
                            await subject.AddRoleAsync(_IRoleRules);
                        
                        log.Info($"CheckRules done for {subject.Username}");
                    }
                }
            }

            chrono.Stop();
            log.Info($"CheckRules OUT in {chrono.ElapsedMilliseconds}ms");
        }

        public async Task UpdateListUser()
        {
            await _client.DownloadUsersAsync(_client.Guilds); // DL all user

            _allUsers = Helper.GetZderLand(_client).Users.ToList();
            _allUsers.RemoveAll(x => x.IsBot);
        }        


        #region Update Live
        //REMOVED
        internal void RulesReactionRemoved(ulong userId)
        {
            var subject = _allUsers.First(x => x.Id == userId);
            subject.RemoveRoleAsync(_IRoleRules);
        }

        //ADDED
        internal async Task RulesReactionAddedAsync(ulong userId)
        {
            var subject = _allUsers.First(x => x.Id == userId);
            await subject.AddRoleAsync(_IRoleRules);

            foreach(IRole separatorRole in _IRolesSeparators)
                await subject.AddRoleAsync(separatorRole);
        }
        #endregion
    }
}
